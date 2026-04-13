using System.Diagnostics.CodeAnalysis;
using TreeDataStructures.Implementations.BST;

namespace TreeDataStructures.Implementations.Splay;

public class SplayTree<TKey, TValue> : BinarySearchTree<TKey, TValue>
{
    protected override BstNode<TKey, TValue> CreateNode(TKey key, TValue value)
        => new(key, value);
    
    protected override void OnNodeAdded(BstNode<TKey, TValue> newNode)
    {
        Splay(newNode);
    }
    
    protected override void OnNodeRemoved(BstNode<TKey, TValue>? parent, BstNode<TKey, TValue>? child)
    {
    }
    
    public override bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        BstNode<TKey, TValue>? node = FindNode(key);
        if (node == null) {
            value = default;
            return false;
        }
        value = node.Value;
        Splay(node);
        return true;
    }

    public override bool Remove(TKey key)
    {
        BstNode<TKey, TValue>? node = FindNode(key);
        if (node == null) {
            return false;
        }
        Splay(node);
        BstNode<TKey, TValue>? leftTree = node.Left;
        BstNode<TKey, TValue>? rightTree = node.Right;
        node.Left = null;
        node.Right = null;
        if (leftTree != null) {
            leftTree.Parent = null;
        }
        if (rightTree != null) {
            rightTree.Parent = null;
        }
        if (leftTree == null) {
            Root = rightTree;
        } else {
            Root = leftTree;
            BstNode<TKey, TValue> leftMax = leftTree;
            while (leftMax.Right != null) {
                leftMax = leftMax.Right;
            }
            Splay(leftMax);
            leftMax.Right = rightTree;
            if (rightTree != null) {
                rightTree.Parent = leftMax;
            }
        }
        Count--;
        return true;
    }

    public override bool ContainsKey(TKey key)
    {
        BstNode<TKey, TValue>? node = FindNode(key);
        if (node != null) {
            Splay(node);
            return true;
        }
        return false;
    }

    private void Splay(BstNode<TKey, TValue> node)
    {
        while (node.Parent != null) {
            BstNode<TKey, TValue> parent = node.Parent;
            BstNode<TKey, TValue>? grandParent = parent.Parent;
            if (grandParent == null) {
                if (node == parent.Left) {
                    RotateRight(parent);
                } else {
                    RotateLeft(parent);
                }
            } else if (node == parent.Left && parent == grandParent.Left) {
                RotateRight(grandParent);
                RotateRight(parent);
            } else if (node == parent.Right && parent == grandParent.Right) {
                RotateLeft(grandParent);
                RotateLeft(parent);
            } else if (node == parent.Right && parent == grandParent.Left) {
                RotateLeft(parent);
                RotateRight(grandParent);
            } else if (node == parent.Left && parent == grandParent.Right) {
                RotateRight(parent);
                RotateLeft(grandParent);
            } else {
                throw new InvalidOperationException("Некорректное дерево");
            }
        }
        Root = node;
    }
    
}