//lexer grammar MermaidLexer;
grammar Mermaid;

//options {
//    superClass = MermaidLexerBase;
//}

fragment DIGIT : [0-9];
fragment NUMBER : DIGIT+ ([,.] DIGIT+)?;

diagram : diagram_type expr+ EOF;
diagram_type : KW_CLASS_DIAGRAM NEWLINE;
//expr : impl;
expr : class | impl | relationship;
//expr : class | impl | note | relationship;
impl : WORD COLON class_attr;
relationship : WORD relation_terminator+ NEWLINE;
relation_terminator : relation WORD (SEMICOLON WORD NEWLINE)?;
relation : (inheritance | composition | aggregation | association | link_solid | dependency | realization | link_dashed);
inheritance : GT PIPE link_solid;
composition : ASTERISK link_solid;
association : link_solid LT;
aggregation : 'o' link_solid;
link_solid : DASH DASH;
dependency : link_dashed LT;
realization : link_dashed PIPE LT;
link_dashed : DOT DOT;
argument_terminator : COMMA WORD+;
//generic : TILDE argument_list TILDE; // TODO multiple types
//class_name : WORD generic+; // TODO recursion of generics?
argument_list : WORD+ argument_terminator+;
access_modifier : (PLUS | DASH | TILDE | HASH);
method : access_modifier WORD OPEN_PAREN argument_list? CLOSE_PAREN ASTERISK? WORD? ASTERISK? DOLLAR? NEWLINE;
field : access_modifier WORD+ DOLLAR? NEWLINE;
class : KW_CLASS WORD class_impl? NEWLINE;
class_impl : OPEN_BRACE NEWLINE class_attr+ CLOSE_BRACE;
class_attr : field | method;
//class_attr : field | method | note;
//class: KW_CLASS (field | method);
//note : KW_NOTE quote_text;
quote_text : QUOTE WORD* QUOTE;
inline_css : COLON COLON COLON;
annotation : GT GT WORD* LT LT;

//WS  :   [ \t\r\n]+ -> skip ;
WS  :   [ \t\r]+ -> skip ;
//: 'TODO';
KW_NOTE : 'note';
KW_FOR : 'for';
KW_CLASS_DEF : 'classDef';
KW_CLASS_DIAGRAM : 'classDiagram';
KW_CLASS : 'class ';
WORD : [a-zA-Z]+;
PIPE : '|';
NEWLINE : '\n';
DOLLAR : '$';
SEMICOLON : ';';
COLON : ':';
INLINE_CSS : COLON COLON COLON;
LT : '>';
GT : '<';
BEGIN_ANNOTATION : GT GT;
END_ANNOTATION : LT LT;
DOT : '.';
DASH : '-';
PERCENT : '%';
ASTERISK : '*';
PLUS : '+';
HASH : '#';
TILDE : '~';
BACKTICK : '`';
COMMA : ',';
OPEN_BRACE : '{';
CLOSE_BRACE : '}';
OPEN_PAREN : '(';
CLOSE_PAREN  : ')';
OPEN_BRACK : '[';
CLOSE_BRACK : ']';
QUOTE : '"';

// relationships