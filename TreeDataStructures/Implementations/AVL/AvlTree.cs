using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.AVL;

public class AvlTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, AvlNode<TKey, TValue>>
    where TKey : IComparable<TKey>
{
    protected override AvlNode<TKey, TValue> CreateNode(TKey key, TValue value)
        => new(key, value);
    
    protected override void OnNodeAdded(AvlNode<TKey, TValue> newNode)
    {
        AvlNode<TKey, TValue>? current = newNode;
        while (current != null) {
            SetHeight(current);
            int difference = GetDifference(current);
            if (difference > 1) {
                if (GetDifference(current.Left!) < 0) {
                    RotateLeft(current.Left!);
                    SetHeight(current.Left!);
                }
                RotateRight(current);
                SetHeight(current);
                if (current.Parent != null) {
                    SetHeight(current.Parent);
                }
            } else if (difference < -1) {
                if (GetDifference(current.Right!) > 0) {
                    RotateRight(current.Right!);
                    SetHeight(current.Right!);
                }
                RotateLeft(current);
                SetHeight(current);
                if (current.Parent != null) {
                    SetHeight(current.Parent);
                }
            }
            current = current.Parent;
        }
    }

    private void SetHeight(AvlNode<TKey, TValue> node)
    {
        int leftHeight = 0;
        int rightHeight = 0;
        if (node.Left != null) {
            leftHeight = node.Left.Height;
        }
        if (node.Right != null) {
            rightHeight = node.Right.Height;
        }
        node.Height = Math.Max(leftHeight, rightHeight) + 1;
    }

    private int GetDifference(AvlNode<TKey, TValue> node)
    {
        int leftHeight = 0;
        int rightHeight = 0;
        if (node.Left != null) {
            leftHeight = node.Left.Height;
        }
        if (node.Right != null) {
            rightHeight = node.Right.Height;
        }
        return leftHeight - rightHeight;
    }

}