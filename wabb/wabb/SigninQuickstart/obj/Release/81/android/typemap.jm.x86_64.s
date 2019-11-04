	/* Data SHA1: 2fbc1dd9a139cd7533e86a127b8ed05b941df97d */
	.file	"typemap.jm.inc"

	/* Mapping header */
	.section	.data.jm_typemap,"aw",@progbits
	.type	jm_typemap_header, @object
	.p2align	2
	.global	jm_typemap_header
jm_typemap_header:
	/* version */
	.long	1
	/* entry-count */
	.long	463
	/* entry-length */
	.long	210
	/* value-offset */
	.long	93
	.size	jm_typemap_header, 16

	/* Mapping data */
	.type	jm_typemap, @object
	.global	jm_typemap
jm_typemap:
	.size	jm_typemap, 97231
	.include	"typemap.jm.inc"
