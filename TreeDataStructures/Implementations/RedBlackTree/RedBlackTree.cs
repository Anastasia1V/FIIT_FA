using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.RedBlackTree;

public class RedBlackTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, RbNode<TKey, TValue>>
{
    protected override RbNode<TKey, TValue> CreateNode(TKey key, TValue value)
    {
        RbNode<TKey, TValue> createdNode = new RbNode<TKey, TValue>(key, value) {
            Color = RbColor.Red
        };
        return createdNode;
    }
    
    protected override void OnNodeAdded(RbNode<TKey, TValue> newNode)
    {
        RbNode<TKey, TValue> current = newNode;
        while (current.Parent != null && current.Parent.Color == RbColor.Red) {
            RbNode<TKey, TValue> parent = current.Parent;
            if (parent.Parent == null) {
                break;
            }
            RbNode<TKey, TValue> grandParent = parent.Parent;
            if (parent == grandParent.Left) {
                RbNode<TKey, TValue>? parentSibling = grandParent.Right;
                if (parentSibling != null && parentSibling.Color == RbColor.Red) {
                    parent.Color = RbColor.Black;
                    parentSibling.Color = RbColor.Black;
                    grandParent.Color = RbColor.Red;
                    current = grandParent;
                } else {
                    if (current == parent.Right) {
                        RotateLeft(parent);
                        current = parent;
                        parent = current.Parent;
                    }
                    parent.Color = RbColor.Black;
                    grandParent.Color = RbColor.Red;
                    RotateRight(grandParent);
                }
            } else {
                RbNode<TKey, TValue>? parentSibling = grandParent.Left;
                if (parentSibling != null && parentSibling.Color == RbColor.Red) {
                    parent.Color = RbColor.Black;
                    parentSibling.Color = RbColor.Black;
                    grandParent.Color = RbColor.Red;
                    current = grandParent;
                } else {
                    if (current == parent.Left) {
                        RotateRight(parent);
                        current = parent;
                        parent = current.Parent;
                    }
                    parent.Color = RbColor.Black;
                    grandParent.Color = RbColor.Red;
                    RotateLeft(grandParent);
                }
            }
        }
        if (Root != null) {
            Root.Color = RbColor.Black;
        }
    }

    protected override void OnNodeRemoved(RbNode<TKey, TValue>? parent, RbNode<TKey, TValue>? child)
    {
        if (parent == null) {
            return;
        }
        RbNode<TKey, TValue>? current = child;
        RbNode<TKey, TValue>? currentParent = parent;
        while (current != Root && (current == null || current.Color == RbColor.Black)) {
            if (currentParent == null) {
                break;
            }
            if (current == currentParent.Left) {
                RbNode<TKey, TValue>? sibling = currentParent.Right;
                if (sibling != null && sibling.Color == RbColor.Red) {
                    sibling.Color = RbColor.Black;
                    currentParent.Color = RbColor.Red;
                    RotateLeft(currentParent);
                    sibling = currentParent.Right;
                }
                if ((sibling?.Left == null || sibling.Left.Color == RbColor.Black) &&
                    (sibling?.Right == null || sibling.Right.Color == RbColor.Black)) {
                    if (sibling != null) {
                        sibling.Color = RbColor.Red;
                    }
                    current = currentParent;
                    currentParent = current.Parent;
                } else {
                    if (sibling?.Right == null || sibling.Right.Color == RbColor.Black) {
                        if (sibling?.Left != null) {
                            sibling.Left.Color = RbColor.Black;
                        }
                        if (sibling != null) {
                            sibling.Color = RbColor.Red;
                            RotateRight(sibling);
                        }
                        sibling = currentParent.Right;
                    }
                    if (sibling != null) {
                        sibling.Color = currentParent.Color;
                    }
                    currentParent.Color = RbColor.Black;
                    if (sibling?.Right != null) {
                        sibling.Right.Color = RbColor.Black;
                    }
                    RotateLeft(currentParent);
                    current = Root;
                }
            } else {
                RbNode<TKey, TValue>? sibling = currentParent.Left;
                if (sibling != null && sibling.Color == RbColor.Red) {
                    sibling.Color = RbColor.Black;
                    currentParent.Color = RbColor.Red;
                    RotateRight(currentParent);
                    sibling = currentParent.Left;
                }
                if ((sibling?.Left == null || sibling.Left.Color == RbColor.Black) &&
                    (sibling?.Right == null || sibling.Right.Color == RbColor.Black)) {
                    if (sibling != null) {
                        sibling.Color = RbColor.Red;
                    }
                    current = currentParent;
                    currentParent = current.Parent;
                } else {
                    if (sibling?.Left == null || sibling.Left.Color == RbColor.Black) {
                        if (sibling?.Right != null) {
                            sibling.Right.Color = RbColor.Black;
                        }
                        if (sibling != null) {
                            sibling.Color = RbColor.Red;
                            RotateLeft(sibling);
                        }
                        sibling = currentParent.Left;
                    }
                    if (sibling != null) {
                        sibling.Color = currentParent.Color;
                    }
                    currentParent.Color = RbColor.Black;
                    if (sibling?.Left != null) {
                        sibling.Left.Color = RbColor.Black;
                    }
                    RotateRight(currentParent);
                    current = Root;
                }
            }
        }
        if (Root != null) {
            Root.Color = RbColor.Black;
        }
    }
}