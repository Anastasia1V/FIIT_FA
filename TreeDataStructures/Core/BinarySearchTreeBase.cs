using System.Collections;
using System.Diagnostics.CodeAnalysis;
using TreeDataStructures.Interfaces;

namespace TreeDataStructures.Core;

public abstract class BinarySearchTreeBase<TKey, TValue, TNode>(IComparer<TKey>? comparer = null) 
    : ITree<TKey, TValue>
    where TNode : Node<TKey, TValue, TNode>
{
    protected TNode? Root;
    public IComparer<TKey> Comparer { get; protected set; } = comparer ?? Comparer<TKey>.Default; // use it to compare Keys

    public int Count { get; protected set; }
    
    public bool IsReadOnly => false;

    public ICollection<TKey> Keys
    {
        get {
            var keysList = new List<TKey>();
            var iterator = new TreeIterator(Root, TraversalStrategy.InOrder);
            while (iterator.MoveNext()) {
                keysList.Add(iterator.Current.Key);
            } 
            return keysList;
        }
    }
    
    public ICollection<TValue> Values
    {
        get {
            var valuesList = new List<TValue>();
            var iterator = new TreeIterator(Root, TraversalStrategy.InOrder);
            while (iterator.MoveNext()) {
                valuesList.Add(iterator.Current.Value);
            }
            return valuesList;
        }
    }
    
    
    public virtual void Add(TKey key, TValue value)
    {
        TNode createdNode = CreateNode(key, value);
        if (Root == null) {
            Root = createdNode;
            Count++;
            OnNodeAdded(createdNode);
            return;
        }
        TNode? node = Root;
        TNode parent = Root;
        int findPlace = 0;
        while (node != null) {
            parent = node;
            findPlace = Comparer.Compare(key, node.Key);
            if (findPlace < 0) {
                node = node.Left;
            }
            else if (findPlace > 0) {
                node = node.Right;
            }
            else {
                node.Value = value;
                return;
            }
        }
        createdNode.Parent = parent;
        findPlace = Comparer.Compare(key, parent.Key);
        if (findPlace < 0) {
            parent.Left = createdNode;
        }
        else {
            parent.Right = createdNode;
        }
        OnNodeAdded(createdNode);
        Count++;
    }

    
    public virtual bool Remove(TKey key)
    {
        TNode? node = FindNode(key);
        if (node == null) { return false; }

        RemoveNode(node);
        this.Count--;
        return true;
    }
    
    
    protected virtual void RemoveNode(TNode node)
    {
        if (node.Left == null) {
            Transplant(node, node.Right);
            OnNodeRemoved(node.Parent, node.Right);
        }
        else if (node.Right == null) {
            Transplant(node, node.Left);
            OnNodeRemoved(node.Parent, node.Left);
        }
        else {
            TNode rightMin = node.Right;
            while (rightMin.Left != null) {
                rightMin = rightMin.Left;
            }
            if (rightMin.Parent != node) {
                Transplant(rightMin, rightMin.Right);
                OnNodeRemoved(rightMin.Parent, rightMin.Right);
                rightMin.Right = node.Right;
                if (rightMin.Right != null) {
                    rightMin.Right.Parent = rightMin;
                }
            }
            Transplant(node, rightMin);
            OnNodeRemoved(node.Parent, rightMin);
            rightMin.Left = node.Left;
            if (rightMin.Left != null) {
                rightMin.Left.Parent = rightMin;
            }
        }
    }

    public virtual bool ContainsKey(TKey key) => FindNode(key) != null;
    
    public virtual bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        TNode? node = FindNode(key);
        if (node != null)
        {
            value = node.Value;
            return true;
        }
        value = default;
        return false;
    }

    public TValue this[TKey key]
    {
        get => TryGetValue(key, out TValue? val) ? val : throw new KeyNotFoundException();
        set => Add(key, value);
    }

    
    #region Hooks
    
    /// <summary>
    /// Вызывается после успешной вставки
    /// </summary>
    /// <param name="newNode">Узел, который встал на место</param>
    protected virtual void OnNodeAdded(TNode newNode) { }
    
    /// <summary>
    /// Вызывается после удаления. 
    /// </summary>
    /// <param name="parent">Узел, чей ребенок изменился</param>
    /// <param name="child">Узел, который встал на место удаленного</param>
    protected virtual void OnNodeRemoved(TNode? parent, TNode? child) { }
    
    #endregion
    
    
    #region Helpers
    protected abstract TNode CreateNode(TKey key, TValue value);
    
    
    protected TNode? FindNode(TKey key)
    {
        TNode? current = Root;
        while (current != null)
        {
            int cmp = Comparer.Compare(key, current.Key);
            if (cmp == 0) { return current; }
            current = cmp < 0 ? current.Left : current.Right;
        }
        return null;
    }

    protected void RotateLeft(TNode x)
    {
        if (x == null) {
            throw new ArgumentNullException(nameof(x), "Отсутствует узел.");
        }
        TNode? y = x.Right;
        if (y == null) {
            throw new InvalidOperationException("Отсутствует правый потомок.");
        }
        x.Right = y.Left;
        if (y.Left != null) {
            y.Left.Parent = x;
        }
        y.Parent = x.Parent;
        if (x.Parent == null) {
            Root = y;
        } else if (x.IsLeftChild) {
            x.Parent.Left = y;
        } else {
            x.Parent.Right = y;
        }
        y.Left = x;
        x.Parent = y;
    }

    protected void RotateRight(TNode y)
    {
        if (y == null) {
            throw new ArgumentNullException(nameof(y), "Отсутствует узел.");
        }
        TNode? x = y.Left;
        if (x == null) {
            throw new InvalidOperationException("Отсутствует левый потомок.");
        }
        y.Left = x.Right;
        if (x.Right != null) {
            x.Right.Parent = y;
        }
        x.Parent = y.Parent;
        if (y.Parent == null) {
            Root = x;
        } else if (y.IsLeftChild) {
            y.Parent.Left = x;
        } else {
            y.Parent.Right = x;
        }
        x.Right = y;
        y.Parent = x;
    }
    
    protected void RotateBigLeft(TNode x)
    {
        if (x == null) {
            throw new ArgumentNullException(nameof(x), "Отсутствует узел.");
        }
        if (x.Right == null) {
            throw new InvalidOperationException("Отсутствует правый потомок.");
        }
        RotateRight(x.Right);
        RotateLeft(x);
    }
    
    protected void RotateBigRight(TNode y)
    {
        if (y == null) {
            throw new ArgumentNullException(nameof(y), "Отсутствует узел.");
        }
        if (y.Left == null) {
            throw new InvalidOperationException("Отсутствует левый потомок.");
        }
        RotateLeft(y.Left);
        RotateRight(y);
    }
    
    protected void RotateDoubleLeft(TNode x)
    {
        RotateLeft(x);
        RotateLeft(x);
    }
    
    protected void RotateDoubleRight(TNode y)
    {
        RotateRight(y);
        RotateRight(y);
    }
    
    protected void Transplant(TNode u, TNode? v)
    {
        if (u.Parent == null)
        {
            Root = v;
        }
        else if (u.IsLeftChild)
        {
            u.Parent.Left = v;
        }
        else
        {
            u.Parent.Right = v;
        }
        v?.Parent = u.Parent;
    }
    #endregion
    
    public IEnumerable<TreeEntry<TKey, TValue>>  InOrder() {
        return new TreeIterator(Root, TraversalStrategy.InOrder);
    }
    public IEnumerable<TreeEntry<TKey, TValue>>  PreOrder() {
        return new TreeIterator(Root, TraversalStrategy.PreOrder);
    }
    public IEnumerable<TreeEntry<TKey, TValue>>  PostOrder() {
        return new TreeIterator(Root, TraversalStrategy.PostOrder);
    }
    public IEnumerable<TreeEntry<TKey, TValue>>  InOrderReverse() {
        return new TreeIterator(Root, TraversalStrategy.InOrderReverse);
    }
    public IEnumerable<TreeEntry<TKey, TValue>>  PreOrderReverse() {
        return new TreeIterator(Root, TraversalStrategy.PreOrderReverse);
    }
    public IEnumerable<TreeEntry<TKey, TValue>>  PostOrderReverse() {
        return new TreeIterator(Root, TraversalStrategy.PostOrderReverse);
    }
    
    /// <summary>
    /// Внутренний класс-итератор. 
    /// Реализует паттерн Iterator вручную, без yield return (ban).
    /// </summary>
    private struct TreeIterator : 
        IEnumerable<TreeEntry<TKey, TValue>>,
        IEnumerator<TreeEntry<TKey, TValue>>
    {
        // probably add something here
        private readonly TNode? _root;
        private TNode? _currentNode;
        private Stack<TNode> _stack;
        private TNode? _lastVisited;
        private TreeEntry<TKey, TValue> _currentEntry;
        private readonly TraversalStrategy _strategy; // or make it template parameter?
        public TreeIterator(TNode? root, TraversalStrategy strategy) {
            _root = root;
            _currentNode = _root;
            _stack = new Stack<TNode>();
            _lastVisited = null;
            _strategy = strategy;
            if (_strategy == TraversalStrategy.PreOrder || _strategy == TraversalStrategy.PreOrderReverse) {
                if (_root != null) {
                    _stack.Push(_root);
                }
                _currentNode = null;
            }
        }
        
        public IEnumerator<TreeEntry<TKey, TValue>> GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;
        
        public TreeEntry<TKey, TValue> Current {
            get { return _currentEntry; }
        }
        object IEnumerator.Current {
            get { return Current; }
        }
        
        
        public bool MoveNext()
        {
            if (_strategy == TraversalStrategy.InOrder) {
                return MoveNextInOrder();
            }
            if (_strategy == TraversalStrategy.PreOrder) {
                return MoveNextPreOrder();
            }
            if (_strategy == TraversalStrategy.PostOrder) {
                return MoveNextPostOrder();
            }
            if (_strategy == TraversalStrategy.InOrderReverse) {
                return MoveNextInOrderReverse();
            }
            if (_strategy == TraversalStrategy.PreOrderReverse) {
                return MoveNextPreOrderReverse();
            }
            if (_strategy == TraversalStrategy.PostOrderReverse) {
                return MoveNextPostOrderReverse();
            }
            return false;
        }
        
        private bool MoveNextInOrder()
        {
            while (_currentNode != null || _stack.Count > 0) {
                if (_currentNode != null) {
                    _stack.Push(_currentNode);
                    _currentNode = _currentNode.Left;
                } else {
                    TNode node = _stack.Pop();
                    _currentEntry = new TreeEntry<TKey, TValue>(
                        node.Key,
                        node.Value,
                        GetHeight(node)
                    );
                    _currentNode = node.Right;
                    return true;
                }
            }
            return false;
        }

        private bool MoveNextPreOrder()
        {
            if (_stack.Count == 0) {
                return false;
            }
            TNode node = _stack.Pop();
            if (node.Right != null) {
                _stack.Push(node.Right);
            }
            if (node.Left != null) {
                _stack.Push(node.Left);
            }
            _currentEntry = new TreeEntry<TKey, TValue>(
                node.Key,
                node.Value,
                GetHeight(node)
            );
            return true;
        }

        private bool MoveNextPostOrder()
        {
            while (_currentNode != null || _stack.Count > 0) {
                if (_currentNode != null) {
                    _stack.Push(_currentNode);
                    _currentNode = _currentNode.Left;
                } else {
                    TNode node = _stack.Peek();
                    if (node.Right != null && _lastVisited != node.Right) {
                        _currentNode = node.Right;
                    } else {
                        _stack.Pop();
                        _currentEntry = new TreeEntry<TKey, TValue>(
                            node.Key,
                            node.Value,
                            GetHeight(node)
                        );
                        _lastVisited = node;
                        return true;
                    }
                }
            }
            return false;
        }

        private bool MoveNextInOrderReverse()
        {
            while (_currentNode != null || _stack.Count > 0) {
                if (_currentNode != null) {
                    _stack.Push(_currentNode);
                    _currentNode = _currentNode.Right;
                } else {
                    TNode node = _stack.Pop();
                    _currentEntry = new TreeEntry<TKey, TValue>(
                        node.Key,
                        node.Value,
                        GetHeight(node)
                    );
                    _currentNode = node.Left;
                    return true;
                }
            }
            return false;
        }

        private bool MoveNextPreOrderReverse()
        {
            if (_stack.Count == 0) {
                return false;
            }
            TNode node = _stack.Pop();
            if (node.Left != null) {
                _stack.Push(node.Left);
            }
            if (node.Right != null) {
                _stack.Push(node.Right);
            }
            _currentEntry = new TreeEntry<TKey, TValue>(
                node.Key,
                node.Value,
                GetHeight(node)
            );
            return true;
        }

        private bool MoveNextPostOrderReverse()
        {
            while (_currentNode != null || _stack.Count > 0) {
                if (_currentNode != null) {
                    _stack.Push(_currentNode);
                    _currentNode = _currentNode.Right;
                } else {
                    TNode node = _stack.Peek();
                    if (node.Left != null && _lastVisited != node.Left) {
                        _currentNode = node.Left;
                    } else {
                        _stack.Pop();
                        _currentEntry = new TreeEntry<TKey, TValue>(
                            node.Key,
                            node.Value,
                            GetHeight(node)
                        );
                        _lastVisited = node;
                        return true;
                    }
                }
            }
            return false;
        }

        private int GetHeight(TNode? node)
        {
            if (node == null) {
                return 0;
            }
            int left = GetHeight(node.Left);
            int right = GetHeight(node.Right);
            return Math.Max(left, right) + 1;
        }

        public void Reset()
        {
            _stack.Clear();
            _currentNode = _root;
            if (_strategy == TraversalStrategy.PreOrder || _strategy == TraversalStrategy.PreOrderReverse) {
                if (_root != null) {
                    _stack.Push(_root);
                }
                _currentNode = null;
            }
            _lastVisited = null;
        }

        public void Dispose()
        {
            // TODO release managed resources here
        }
    }
    
    
    private enum TraversalStrategy { InOrder, PreOrder, PostOrder, InOrderReverse, PreOrderReverse, PostOrderReverse }
    
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        var iterator = new TreeIterator(Root, TraversalStrategy.InOrder);
        return new KeyAndValue(iterator);
    }
    
    private sealed class KeyAndValue : IEnumerator<KeyValuePair<TKey, TValue>>
    {
        private TreeIterator _iterator;

        public KeyAndValue(TreeIterator iterator) {
            _iterator = iterator;
        }

        public KeyValuePair<TKey, TValue> Current {
            get {
                TreeEntry<TKey, TValue> entry = _iterator.Current;
                return new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
            }
        }

        object IEnumerator.Current => Current;

        public bool MoveNext() {
            return _iterator.MoveNext();
        }

        public void Reset() {
            _iterator.Reset();
        }

        public void Dispose() {
            _iterator.Dispose();
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
    public void Clear() { Root = null; Count = 0; }
    public bool Contains(KeyValuePair<TKey, TValue> item) => ContainsKey(item.Key);
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        if (array == null) {
            throw new ArgumentNullException(nameof(array), "Отсутствует массив.");
        }
        else if (arrayIndex < 0) {
            throw new ArgumentOutOfRangeException(nameof(arrayIndex), "Отрицательный индекс.");
        }
        else if (array.Length - arrayIndex < Count) {
            throw new ArgumentException("Недостаточная длина массива.");
        } else {
            var iterator = new TreeIterator(Root, TraversalStrategy.InOrder);
            int index = arrayIndex;
            while (iterator.MoveNext()) {
                TreeEntry<TKey, TValue> entry = iterator.Current;
                array[index] = new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
                index++;
            }
        }
    }
    public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);
}