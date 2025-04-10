namespace Syntex;

public class MermaidExpressionVisitor
{
    public virtual void Visit(MermaidDiagramTypeExpression expression) {  }
    public virtual void Visit(MermaidDirectionExpression expression) {  }
    public virtual void Visit(MermaidAnnotationExpression expression) {  }
    public virtual void Visit(MermaidMethodExpression expression) {  }
    public virtual void Visit(MermaidNoteExpression expression) {  }
    public virtual void Visit(MermaidRelationshipExpression expression) {  }
    public virtual void Visit(MermaidArgumentExpression expression) {  }
    public virtual void Visit(MermaidFieldExpression expression) {  }
    public virtual void Visit(MermaidClassExpression expression) {  }
}