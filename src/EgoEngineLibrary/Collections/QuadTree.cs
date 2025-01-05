using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace EgoEngineLibrary.Collections;

// References
// https://eugene-eeo.github.io/blog/qd-quadtree.html
// https://lisyarus.github.io/blog/posts/building-a-quadtree.html
// https://badecho.com/index.php/2023/01/14/fast-simple-quadtree/

public abstract class QuadTree<TSelf, TData> where TSelf : QuadTree<TSelf, TData>
{
    private readonly int _maxDepth;
    protected QuadTreeBounds Bounds { get; }

    public abstract IReadOnlyCollection<TData> Elements { get; }
    
    // TL, TR
    // BL, BR
    protected TSelf[]? Children { get; private set; }
    
    [MemberNotNullWhen(false, nameof(Children))]
    public bool IsLeaf => Children is null || Children.Length == 0;

    public int Level { get; protected init; }

    protected QuadTree(QuadTreeBounds bounds, int maxDepth)
    {
        _maxDepth = maxDepth;
        Bounds = bounds;
    }

    public bool Add(TData data)
    {
        if (IsLeaf)
        {
            if (!AddElement(data))
            {
                return false;
            }
            
            if (ShouldSplit())
            {
                Split();
            }

            return true;
        }

        var added = false;
        foreach (var child in Children)
        {
            var addedChild = child.Add(data);
            added = added || addedChild;
        }

        return added;
    }
    
    private void Split()
    {
        // If we're not a leaf node, then we're already split.
        if (!IsLeaf)
        {
            return;
        }

        // Splitting is only allowed if it doesn't cause us to exceed our maximum depth.
        if (Level >= _maxDepth)
        {
            return;
        }

        Children =
        [
            CreateChild(0),
            CreateChild(1),
            CreateChild(2),
            CreateChild(3)
        ];

        foreach (var data in Elements)
        {
            Add(data);
        }
        
        ClearElements();
        return;

        TSelf CreateChild(int childSelect)
        {
            var min = Bounds.Min;
            var max = Bounds.Max;
            var halfSize = Bounds.HalfSize;
            float width = halfSize.X;
            if ((childSelect & 0b01) != 0)
            {
                min.X += width;
            }
            else
            {
                max.X -= width;
            }

            width = halfSize.Y;
            if ((childSelect & 0b10) != 0)
            {
                max.Y -= width;
            }
            else
            {
                min.Y += width;
            }

            return this.CreateChild(new QuadTreeBounds(min, max));
        }
    }

    public abstract IEnumerable<TSelf> Traverse();
    
    protected abstract TSelf CreateChild(QuadTreeBounds bounds);
    
    protected abstract bool AddElement(TData data);
    
    protected abstract void ClearElements();
    
    protected abstract bool ShouldSplit();

    public IEnumerable<TSelf> TraverseFromBottomLeft()
    {
        if (IsLeaf)
        {
            yield return (TSelf)this;
            yield break;
        }

        // Traverse BL, TL, BR, TR, depth first
        foreach (var child in Children[2].TraverseFromBottomLeft())
        {
            yield return child;
        }

        foreach (var child in Children[0].TraverseFromBottomLeft())
        {
            yield return child;
        }

        foreach (var child in Children[3].TraverseFromBottomLeft())
        {
            yield return child;
        }

        foreach (var child in Children[1].TraverseFromBottomLeft())
        {
            yield return child;
        }
    }
}
