// SpicaML grammar for ANTLR v3
//  
// Philipp A. Baer <phbaer@npw.net>
//
// 2006-2009 by Carpe Noctem Robotic Soccer
// Distributed Systems Group, Kassel University, Germany
// http://carpenoctem.das-lab.net/
//
// 2009 by DFKI RIC Bremen
// http://robotik.dfki-bremen.de/

grammar SpicaML;
options { language = CSharp2; output = AST; }

// Entrypoint for Aastra.
model
	:	( model_namespace | model_include )* ( struct_spec | flow_spec )*
	;

// Grammar rules for the global SpicaML model structure
model_namespace
	:	NAMESPACE misc_id ( '.' misc_id )* ';'
		-> ^( NAMESPACE misc_id+ )
	;
model_include
@init {
    CommonTree includetree = null;
    string path = null;
}
	:	INCLUDE f=T_STRING ';' {
            {
                string file = $f.text.Trim(new char[] { '"' });

                if (System.IO.Path.IsPathRooted($f.text))
                {
                    path = file;
                }
                else
                {
                    path = System.IO.Path.GetDirectoryName(SourceName);
                    path = System.IO.Path.Combine(path, file);
                }

                if (!System.IO.File.Exists(path))
                {
                    throw new Castor.CException("File not found: {0} (included in {1})", path, SourceName);
                }

                try
                {
                    ICharStream inner_instr = new ANTLRFileStream(path);
                    SpicaMLLexer inner_lex = new SpicaMLLexer(inner_instr);
                    CommonTokenStream inner_tokens = new CommonTokenStream(inner_lex);
                    SpicaMLParser inner_parser = new SpicaMLParser(inner_tokens);
                    includetree = (CommonTree)(inner_parser.model().Tree);
                }
                catch (Exception e)
                {
                    throw new Castor.CException(e, "Error processing: {0} (included in {1})", path, SourceName);
                }
            }
        }
		-> ^(INCLUDE[path] {includetree})
	;

// Grammar rules for the data structure specification
struct_spec
	:	( struct_enum | struct_container )
	;
struct_sig
	:	misc_type struct_super_type? misc_ann?
	;
struct_super_type
	:	':' misc_type ( ',' misc_type )*
		-> ^( INHERIT misc_type+ )
	;
struct_enum
	:	ENUM struct_sig ':' misc_id_prim '{' struct_enum_value+ '}'
		-> ^( ENUM struct_sig misc_id_prim struct_enum_value+ )
	;
struct_enum_value
	:	ID ( '=' misc_value )? ';' -> ^( ITEM ID misc_value? )
	;
struct_container
	:	STRUCT struct_sig ( '{' struct_block_field* '}' | ';' )
		-> ^( STRUCT struct_sig struct_block_field* )
	;
struct_block_field
	:	struct_block_field_type struct_block_field_name
		-> ^( FIELD struct_block_field_type struct_block_field_name )
	;
struct_block_field_type
	:	misc_type struct_block_field_array?
	;
struct_block_field_name
	:	misc_id_all struct_block_field_default_value? misc_ann? ';'
		-> ^( NAME misc_id_all ) struct_block_field_default_value? misc_ann?
	;
struct_block_field_default_value
	:	'=' misc_value
		-> ^( DEFAULT misc_value )
	;
struct_block_field_array
	:	(
		 '[' ( T_INT ( ',' T_INT )* )? ']' -> ^( ARRAY T_INT* ) |
		 '[]' -> ^( ARRAY )
		)
	;

// Grammar rules for the data flow specification
flow_spec
	: flow_module
	;
flow_module
	:	MODULE misc_type misc_ann? '{'
		(
		 flow_module_pub_default |
		 flow_module_pub |
		 flow_module_sub_default |
		 flow_module_sub_extract_default |
		 flow_module_sub )+
		'}'
		-> ^( MODULE misc_type misc_ann?
			flow_module_pub_default*
			flow_module_pub*
			flow_module_sub_default*
			flow_module_sub_extract_default*
			flow_module_sub* )
	;
flow_module_pub_default
	:	PUB '.' flow_module_pub_ann_kv ';'
		-> ^( PUBDEF flow_module_pub_ann_kv )
	;
flow_module_pub
	:	PUB misc_type flow_module_pub_ann? ';'
		-> ^( PUB misc_type flow_module_pub_ann? )
	;
flow_module_pub_ann
	:	'[' flow_module_pub_ann_kv ( ';' flow_module_pub_ann_kv )* ';'? ']'
		-> flow_module_pub_ann_kv+
	;
flow_module_pub_ann_kv
	:	(
		 flow_module_misc_ann_dmc |
		 flow_module_misc_ann_period |
		 flow_module_pub_ann_dst |
 		 flow_module_misc_ann_ttl
		)
	;
flow_module_pub_ann_dst
	:	( 'dst' '=' flow_module_misc_ann_card? ( flow_module_misc_ann_scope_element ( ',' flow_module_misc_ann_scope_element )* ) )
		-> ^( DST flow_module_misc_ann_card? flow_module_misc_ann_scope_element+ )
	;
flow_module_sub
	:	SUB misc_type flow_module_sub_ann ';'
		-> ^( SUB misc_type flow_module_sub_ann )
	;
flow_module_sub_default
	:	SUB '.' flow_module_sub_ann_kv_no_extract ';'
		-> ^( SUBDEF flow_module_sub_ann_kv_no_extract )
	;
flow_module_sub_extract_default
	:	SUB '.' EXTRACT '.' flow_module_sub_ann_extract_spec_ann_kv ';'
		-> ^( SUBEXDEF flow_module_sub_ann_extract_spec_ann_kv )
	;
flow_module_sub_variables
	:	flow_module_misc_ann_dmc |
		flow_module_misc_ann_period
	;
flow_module_sub_ann
	:	'[' flow_module_sub_ann_kv ( ';' flow_module_sub_ann_kv )* ';'? ']'
		-> flow_module_sub_ann_kv+
	;
flow_module_sub_ann_kv
	:	(
		 flow_module_sub_ann_kv_no_extract |
		 flow_module_sub_ann_extract
		)
	;
flow_module_sub_ann_kv_no_extract
	:	(
		 flow_module_misc_ann_dmc |
		 flow_module_sub_ann_src |
		 flow_module_misc_ann_period |
		 flow_module_sub_ann_type |
		 flow_module_sub_ann_field |
		 flow_module_misc_ann_ttl
		)
	;
flow_module_misc_ann_dmc
	:	( 'dmc' '=' ( QUEUE | RINGBUFFER ) ( '/' T_UINT )? )
		-> ^( DMC QUEUE? RINGBUFFER? T_UINT? )
	;
flow_module_misc_ann_period
	:	( 'period' '=' T_TIME )
		-> ^( PERIOD T_TIME )
	;
flow_module_sub_ann_src
	:	( 'src' '=' flow_module_misc_ann_card? ( flow_module_misc_ann_scope_element ( ',' flow_module_misc_ann_scope_element )* ) )
		-> ^( SRC flow_module_misc_ann_card? flow_module_misc_ann_scope_element+ )
	;
flow_module_misc_ann_card
	:	( flow_module_misc_ann_card_item ( RANGE flow_module_misc_ann_card_item )? ) '|'
		-> ^( CARD flow_module_misc_ann_card_item+ )
	;
flow_module_misc_ann_card_item
	:	( T_UINT | 'n' )
	;
flow_module_misc_ann_scope_element
	:	( flow_module_misc_ann_scope_def | flow_module_misc_ann_scope_undef )
	;
flow_module_misc_ann_scope_def
	:	( misc_id ':' )? misc_id '/' misc_id
		-> ^( SPEC misc_id+ )
	;
flow_module_misc_ann_scope_undef
	:	( '!' ( misc_id ':' )? misc_id '/' misc_id )
		-> ^( NOSPEC misc_id+ )
	;
flow_module_sub_ann_type
	:	( 'type' '=' misc_id )
		-> ^( TYPE misc_id )
	;
flow_module_sub_ann_field
	:	( 'field' '=' misc_id )
		-> ^( FIELD misc_id )
	;
flow_module_misc_ann_ttl
	:	( 'ttl' '=' T_TIME )
		-> ^( TTL T_TIME )
	;
flow_module_sub_ann_extract
	:	( 'extract' '=' flow_module_sub_ann_extract_spec )
		-> ^( EXTRACT flow_module_sub_ann_extract_spec )
	;
flow_module_sub_ann_extract_spec
	:	flow_module_sub_ann_extract_field ':' misc_id_all flow_module_sub_ann_extract_spec_ann?
		-> flow_module_sub_ann_extract_field ^( TYPE misc_id_all ) flow_module_sub_ann_extract_spec_ann?
	;
flow_module_sub_ann_extract_field
	:	misc_id_all ( '.' misc_id_all )*
		-> ^( FIELDSPEC misc_id_all+ )
	;
flow_module_sub_ann_extract_spec_ann
	:	'[' flow_module_sub_ann_extract_spec_ann_kv ( ';' flow_module_sub_ann_extract_spec_ann_kv )* ';'? ']'
		-> flow_module_sub_ann_extract_spec_ann_kv+
	;
flow_module_sub_ann_extract_spec_ann_kv
	:	(
		 flow_module_misc_ann_dmc |
		 flow_module_misc_ann_ttl
		)
	;

// Grammar rules for miscellaneous purposes
misc_ann
	:	( '[' misc_ann_kv ( ';' misc_ann_kv )* ']' ) -> misc_ann_kv*
	;
misc_ann_kv
	:	( misc_ann_kv_spec_address | misc_ann_kv_generic )+
	;
misc_ann_kv_spec_address
	:	( ADDRESS '=' T_STRING ) -> ^( ANNOTATION ADDRESS T_STRING )
	;
misc_ann_kv_generic
	:	misc_id ( '=' ( misc_value ( ',' misc_value )* ) )?
		-> ^( ANNOTATION misc_id misc_value* )
	;
misc_value
	:	(
		 T_UINT | T_INT | T_FLOAT | T_TIME | T_STRING | TRUE | FALSE |
		 misc_urn_id | misc_type
		)
	;
misc_type_container
	:	misc_id
		-> ^( TYPENAME misc_id )
	;
misc_type
	:	misc_id_prim | misc_type_container
	;
misc_urn_id
	:	( '#' )? misc_id_all ( ':' misc_id_all )+
		-> ^( URN '#'? misc_id_all+ )
	;
misc_urn_prefix
	:	( '#' | misc_id )
	;
misc_urn_id_open
	:	misc_urn_id ( ':' )? -> misc_urn_id
	;
misc_id
	:
		URN | INCLUDE | HEADER | STRUCT | MESSAGE | SRC | NOSRC |
		DEFAULT | INHERIT | ARRAY | ENUM | ITEM | FIELD | PERIOD |
		ANNOTATION | VECTOR | VIEW | DMC | RINGBUFFER | QUEUE |
		MODULE | PUB | SUB | PUBDEF | SUBDEF | ANNOUNCE | STATIC | FILTER | CALL |
		FUNCTION | VARIABLE | VALUE | ARGS | EALL | ASSIGN | BODY |
		NAME | IDENTIFIER | GROUP | TYPENAME | NAMESPACE | SCHEME |
		OTO | OTMS | OTMPS | PEERS | DEFINE | CARD | PROTO | SPEC | NOSPEC |ID
	;
misc_id_all
	:	misc_id | BOOL | UINT8 | UINT16 | UINT32 | UINT64 | INT8 |
		INT16 | INT32 | INT64 | FLOAT | DOUBLE | STRING | RANGE | EOL
	;
misc_id_prim
	:	(
		 BOOL | UINT8 | UINT16 | UINT32 | UINT64 | INT8 | INT16 |
		 INT32 | INT64 | FLOAT | DOUBLE | STRING | ADDRESS
		)
	;
misc_version
	:	major=T_UINT '.' minor=T_UINT '.' revision=T_UINT
		-> ^( $major $minor $revision )
	;

INCLUDE    : 'include'    ; SRC       : 'src'       ; NOSRC     : 'nosrc'      ;
STRUCT     : 'struct'     ; HEADER    : 'header'    ; MESSAGE   : 'message'    ;
DEFAULT    : 'default'    ; INHERIT   : 'inherit'   ; PERIOD    : 'period'     ;
ARRAY      : 'array'      ; ENUM      : 'enum'      ; ITEM      : 'item'       ;
FIELD      : 'field'      ; VIEW      : 'view'      ; URN       : 'urn'        ;
ANNOTATION : 'annotation' ; VECTOR    : 'vector'    ; DMC       : 'dmc'        ;
RINGBUFFER : 'ringbuffer' ; QUEUE     : 'queue'     ; MODULE    : 'module'     ;
ANNOUNCE   : 'announce'   ; PUB       : 'pub'       ; SUB       : 'sub'        ;
PUBDEF     : 'pubdef'     ; SUBDEF    : 'subdef'    ; EXTRACT   : 'extract'    ;
STATIC     : 'static'     ; FILTER    : 'filter'    ; CALL      : 'call'       ;
FUNCTION   : 'function'   ; VARIABLE  : 'variable'  ; VALUE     : 'value'      ;
ARGS       : 'args'       ; EALL      : 'eall'      ; ASSIGN    : 'assign'     ;
BODY       : 'body'       ; IF        : 'if'        ; FOR       : 'for'        ;
PREDEFINED : 'predefined' ; NAME      : 'name'      ; GROUP     : 'group'      ;
IDENTIFIER : 'identifier' ; TYPENAME  : 'typename'  ; PEERS     : 'peers'      ;
DEFINE     : 'define'     ; NAMESPACE : 'namespace' ; TTL       : 'ttl'        ;
SCHEME     : 'scheme'     ; OTO       : 'oto'       ; OTMS      : 'otm/stream' ;
OTMPS      : 'otm/pubsub' ; TRUE      : 'true'      ; FALSE     : 'false'      ;
BOOL       : 'bool'       ; UINT8     : 'uint8'     ; UINT16    : 'uint16'     ;
UINT32     : 'uint32'     ; UINT64    : 'uint64'    ; INT8      : 'int8'       ;
INT16      : 'int16'      ; INT32     : 'int32'     ; INT64     : 'int64'      ;
FLOAT      : 'float'      ; DOUBLE    : 'double'    ; STRING    : 'string'     ;
ADDRESS    : 'address'    ; EOL       : ';'         ; TYPE      : 'type'       ;
SUBEXDEF   : 'subexdef'   ; CARD      : 'card'      ; PROTO     : 'proto'      ;
RANGE      : '-'          ; SPEC      : 'spec'      ; NOSPEC    : 'nospec'     ;
DST        : 'dst'        ; FIELDSPEC : 'fieldspec' ;

fragment UINT
	:	( '0'..'9' )+ ;

ID
	:	('a'..'z'|'A'..'Z'|'_') ('a'..'z'|'A'..'Z'|'0'..'9'|'_')*
	;
T_UINT
	:	UINT
	;
T_INT
	:	('-'|'+')? UINT
	;
T_FLOAT
	:	( T_INT '.' UINT? | '.' UINT)
	;
T_STRING
	:	'"' (ESC | ~('\\'|'"'))* '"'
	;
T_TIME
	:	( ( T_INT | T_FLOAT ) ( 'm' | 's' | 'ms' | 'us' | 'ns' ) )
	;

protected
ESC
	:	'\\' ('n' | 'r')
	;
WS
	:	(' '|'\r'|'\t'|'\u000C'|'\n') { $channel=HIDDEN; }
	;

COMMENT
	:	(
			'/*' (options {greedy=false;} : .)* '*/' { $channel=HIDDEN; } |
			'//' ~('\r'|'\n')* { $channel=HIDDEN; }
		)
	;

