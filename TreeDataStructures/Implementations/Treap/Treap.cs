using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.Treap;

public class Treap<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, TreapNode<TKey, TValue>>
{
    /// <summary>
    /// Разрезает дерево с корнем <paramref name="root"/> на два поддерева:
    /// Left: все ключи <= <paramref name="key"/>
    /// Right: все ключи > <paramref name="key"/>
    /// </summary>
    protected virtual (TreapNode<TKey, TValue>? Left, TreapNode<TKey, TValue>? Right) Split(TreapNode<TKey, TValue>? root, TKey key)
    {
        if (root == null) {
            return (null, null);
        }
        if (Comparer.Compare(root.Key, key) <= 0) {
            var (leftTree, rightTree) = Split(root.Right, key);
            root.Right = leftTree;
            if (leftTree != null) {
                leftTree.Parent = root;
            }
            return (root, rightTree);
        } else {
            var (leftTree, rightTree) = Split(root.Left, key);
            root.Left = rightTree;
            if (rightTree != null) {
                rightTree.Parent = root;
            }
            return (leftTree, root);
        }
    }

    /// <summary>
    /// Сливает два дерева в одно.
    /// Важное условие: все ключи в <paramref name="left"/> должны быть меньше ключей в <paramref name="right"/>.
    /// Слияние происходит на основе Priority (куча).
    /// </summary>
    protected virtual TreapNode<TKey, TValue>? Merge(TreapNode<TKey, TValue>? left, TreapNode<TKey, TValue>? right)
    {
        if (left == null) {
            return right;
        }
        if (right == null) {
            return left;
        }
        if (left.Priority > right.Priority) {
            left.Right = Merge(left.Right, right);
            if (left.Right != null) {
                left.Right.Parent = left;
            }
            return left;
        } else {
            right.Left = Merge(left, right.Left);
            if (right.Left != null) {
                right.Left.Parent = right;
            }
            return right;
        }
    }
    
    public override void Add(TKey key, TValue value)
    {
        if (ContainsKey(key)) {
            TreapNode<TKey, TValue> node = FindNode(key)!;
            node.Value = value;
            return;
        }
        TreapNode<TKey, TValue> createdNode = CreateNode(key, value);
        var (leftTree, rightTree) = Split(Root, key);
        var newLeftTree = Merge(leftTree, createdNode);
        Root = Merge(newLeftTree, rightTree);
        if (Root != null) {
            Root.Parent = null;
        }
        Count++;
    }

    public override bool Remove(TKey key)
    {
        TreapNode<TKey, TValue>? keyNode = FindNode(key);
        if (keyNode == null) {
            return false;
        }
        TreapNode<TKey, TValue>? subtree = Merge(keyNode.Left, keyNode.Right);
        Transplant(keyNode, subtree);
        Count--;
        return true;
    }

    protected override TreapNode<TKey, TValue> CreateNode(TKey key, TValue value)
    {
        return new TreapNode<TKey, TValue>(key, value);
    }
    protected override void OnNodeAdded(TreapNode<TKey, TValue> newNode)
    {
    }
    
    protected override void OnNodeRemoved(TreapNode<TKey, TValue>? parent, TreapNode<TKey, TValue>? child)
    {
    }
    
}