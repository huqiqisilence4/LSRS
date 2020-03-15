using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace TreeViewFilterDemo
{
    public class TreeViewFilterCtrl<T> where T : TreeViewFilterBaseMdl,new()
    {
        List<T> listForSort = new List<T>();

        List<T> listForShown = new List<T>();

        List<T> listForHide = new List<T>();

        Dictionary<int, T> childNodes = new Dictionary<int, T>();

        private readonly TreeView currentTree = null;

        private readonly List<T> currentList = new List<T>();

        private TreeViewFilterMasterFiledMod filedMod = TreeViewFilterMasterFiledMod.FiledChildOnly;

        public delegate bool FiledHandle(T t,string key);

        public event FiledHandle FiledEvent;

        public TreeViewFilterCtrl(TreeView tv, List<T> dataList)
        {
            this.currentTree = tv;
            this.currentList.AddRange(dataList);
            this.InitTree();
        }

        private void InitTree()
        {

            FullTree(this.currentTree,this.currentList);
            listForShown.AddRange(this.currentList);
            List<TreeNode> sortList = new List<TreeNode>();
            InitListForSort(this.currentTree, sortList);
            listForSort = sortList.Select(t => t.Tag as T).ToList();
            //this.SortTree();
        }

        private static void FullTree(TreeView tv, List<T> list)
        {
      
          
            try
            {

                tv.SuspendLayout();

                List<T> childList = new List<T>();
                foreach (var item in list)
                {
                    if (item.Pid == -1)
                    {
                        var tn = CreateNode(item);
                        tv.Nodes.Add(tn);
                        FullTree(tn, list);
                    }
                }
            }
            catch (Exception exc)
            {
                throw exc;
            }
            finally
            {
                tv.ResumeLayout();

            }
       
        }

        private static void FullTree(TreeNode tn, List<T> list)
        {
            T d = tn.Tag as T;

            var childList = list.Where(e => e.Pid == d.Id);

            if (childList.Count() > 0)
            {
                foreach (var item in childList)
                {
                    var tempTn = CreateNode(item);
                    tn.Nodes.Add(tempTn);
                    FullTree(tempTn, list);
                }
            }

        }

        private static TreeNode FindTreeNode(TreeView tv, int id)
        {
            foreach (TreeNode item in tv.Nodes)
            {
                T d = item.Tag as T;
                if (d != null && d.Id == id)
                {
                    return item;
                }
                else
                {
                    if (item.Nodes.Count > 0)
                    {
                        TreeNode tempTn = FindTreeNode(item, id);

                        if (tempTn != null)
                        {
                            return tempTn;
                        }
                    }
                }
            }

            return null;
        }

        private static TreeNode FindTreeNode(TreeNode tn, int id)
        {
            foreach (TreeNode item in tn.Nodes)
            {
                T d = item.Tag as T;

                if (d.Id == id)
                {
                    return item;
                }
                else
                {
                    if (item.Nodes.Count > 0)
                    {
                        TreeNode tempTn = FindTreeNode(item, id);

                        if (tempTn != null)
                        {
                            return tempTn;
                        }
                    }
                }
            }

            return null;
        }

        private static void InitListForSort(TreeView tv, List<TreeNode> sortList)
        {
            foreach (TreeNode item in tv.Nodes)
            {
                sortList.Add(item);
                if (item.Nodes.Count > 0)
                {
                    InitListForSort(item, sortList);
                }
            }
        }

        private static void InitListForSort(TreeNode tn, List<TreeNode> sortList)
        {
            if (tn != null && tn.Nodes.Count > 0)
            {
                foreach (TreeNode item in tn.Nodes)
                {
                    sortList.Add(item);
                    if (item.Nodes.Count > 0)
                    {
                        InitListForSort(item, sortList);
                    }
                }
            }
        }

        private TreeNode AddNode(TreeView tv, T d)
        {
            TreeNode tn = CreateNode(d);
            if (listForShown.Contains(d) == true)
            {
                return FindTreeNode(tv, d.Id);
            }
            else if (listForHide.Contains(d) == true)
            {
                if (listForShown.Where(i => i.Id == d.Pid).Count() > 0)
                {
                    TreeNode ptn = FindTreeNode(tv, d.Pid);
                    if (ptn != null)
                    {
                        ptn.Nodes.Add(tn);
                    }
                    else
                    {
                        tv.Nodes.Add(tn);
                    }
                }
                else if (listForHide.Where(e => e.Id == d.Pid).Count() > 0)
                {
                    T p = listForHide.Where(e => e.Id == d.Pid).First();
                    TreeNode pn = this.AddNode(tv, p);
                    pn.Nodes.Add(tn);
                }
                else
                {
                    tv.Nodes.Add(tn);
                }

                listForHide.Remove(d);

                if (listForShown.Contains(d) == false)
                {
                    listForShown.Add(d);
                }
            }
            else
            {
                throw new Exception("nullNode");
            }

            if (tv.TreeViewNodeSorter == null)
            {
                this.SortTree();
            }

            return tn;
        }

        private void RemoveNode(TreeView tv, T d, bool delParentThenNullNode = true)
        {
            TreeNode tn = FindTreeNode(tv, d.Id);

            if (tn == null || this.listForHide.Contains(d) == true)
            {
                return;
            }
            else if (tn.Nodes.Count > 0)
            {
                return;
            }
            else
            {

                if (tn != null && tn.Nodes.Count > 0)
                {
                    foreach (TreeNode item in tn.Nodes)
                    {
                        if (item == null)
                        {
                            continue;
                        }
                        T tempD = item.Tag as T;
                        this.RemoveNode(tv, tempD, false);
                    }
                }

                T tempd = tn.Tag as T;
                if (listForShown.Contains(tempd) == true)
                {
                    listForShown.Remove(tempd);
                }
                listForHide.Add(tempd);

                if (delParentThenNullNode == true)
                {
                    TreeNode tempTn = tn;
                    while (true)
                    {
                        if (tn.Parent != null && tn.Parent.Nodes.Count == 1)
                        {
                            TreeNode ptn = tn.Parent;
                            tn.Remove();
                            tn = ptn;
                        }
                        else
                        {
                            tn.Remove();
                            break;
                        }

                        T tempDelD = tn.Tag as T;
                        if (this.listForShown.Contains(tempDelD) == true)
                        {
                            this.listForShown.Remove(tempDelD);
                        }

                        if (this.listForHide.Contains(tempDelD) == false)
                        {
                            this.listForHide.Add(tempDelD);
                        }

                    }
                }
                else
                {
                    tn.Remove();
                }
            }
        }

        private void SortTree()
        {
            try
            {
                this.currentTree.SuspendLayout();
                this.currentTree.TreeViewNodeSorter = new sorter(this.listForSort);
                this.currentTree.Sort();
            }
            catch (Exception exc)
            {
                throw exc;
            }
            finally
            {
                this.currentTree.ResumeLayout();
            }
        }

        private static TreeNode CreateNode(T d)
        {
            TreeNode tn = new TreeNode();
            tn.Text = d.Name;
            tn.Tag = d;
            return tn;
        }

        private bool isChildNode(T d)
        {
            if (d == null || d.Id < 0)
            {
                return false;
            }

            if (childNodes.ContainsKey(d.Id) == true)
            {
                return true;
            }

            if (this.listForHide.Where(l => l.Pid == d.Id).Count() > 0 || this.listForShown.Where(l => l.Pid == d.Id).Count() > 0)
            {
                return false;
            }
            else
            {
                childNodes.Add(d.Id, d);
                return true;
            }
        }

        public void UcFiled(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                var chidList = this.listForHide.Where(l => this.isChildNode(l)).ToList();

                foreach (var item in chidList)
                {
                    this.AddNode(this.currentTree, item);
                }

                this.currentTree.CollapseAll();
            }

            List<T> del = this.listForShown.Where(i => 
                (this.FiledEvent == null ? i.Name.Contains(key) == false : this.FiledEvent(i,key) == false)
                && (this.isChildNode(i) == true || this.filedMod != TreeViewFilterMasterFiledMod.FiledChildOnly)).ToList();

            foreach (var item in del)
            {
                this.RemoveNode(this.currentTree, item);
            }

            List<T> adds = this.listForHide.Where(i => 
                (this.FiledEvent == null?  i.Name.Contains(key) == true : this.FiledEvent(i,key) == true)
                && (this.isChildNode(i) == true || this.filedMod != TreeViewFilterMasterFiledMod.FiledChildOnly)).ToList();

            foreach (var item in adds)
            {
                this.AddNode(this.currentTree, item);
            }

            if (this.currentTree.Nodes.Count < 5)
            {
                this.currentTree.ExpandAll();

                try
                {
                    if (this.currentTree.SelectedNode == null)
                    {
                        var tn = this.listForShown.Where(o => this.isChildNode(o) == true).First();
                        var node = FindTreeNode(this.currentTree, tn.Id);
                        this.currentTree.SelectedNode = node;
                        this.currentTree.HideSelection = false;
                    }

                }
                catch { }
            }
        }

        public void UcSetFiledMod(TreeViewFilterMasterFiledMod mod)
        {
            this.filedMod = mod;
        }

        private class sorter : IComparer
        {
            IList<T> sorterList = null;
            public sorter(IList<T> sorterList)
            {
                this.sorterList = sorterList;
            }



            #region IComparer 成员

            public int Compare(Object x, Object y)
            {
                if (this.sorterList != null && this.sorterList.Count > 0 && x is TreeNode && y is TreeNode)
                {
                    TreeNode tx = x as TreeNode;
                    TreeNode ty = y as TreeNode;

                    T dx = tx.Tag as T;
                    T dy = ty.Tag as T;

                    int ix = this.sorterList.IndexOf(dx);
                    int iy = this.sorterList.IndexOf(dy);
                    return ix.CompareTo(iy);
                }
                else
                {
                    return 1;
                }
            }

            #endregion
        }

    }

    public class TreeViewFilterBaseMdl
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Pid { get; set; }
        public Object Tag { get; set; }
        public override string ToString()
        {
            return Id.ToString() + "," + Name + "," + Pid.ToString();
        }
    }

    public enum TreeViewFilterMasterFiledMod
    {
        /// <summary>
        /// 只筛选 叶子节点
        /// </summary>
        FiledChildOnly,
        /// <summary>
        /// 叶子节点和父节点都筛选
        /// </summary>
        FiledChildAndParent
    }
}
