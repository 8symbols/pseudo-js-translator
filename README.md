# PseudoJsTranslator

PseudoJsTranslator is a translator from some subset of JavaScript into a nothing (but it's just for a while).

## Installation

Just open the solution with a Visual Studio. All required packages will be installed automatically.

## Grammar

The grammar file was taken from [the Antlr Project's repository](https://github.com/antlr/grammars-v4/tree/master/javascript/ecmascript/CSharpSharwell) and slightly modified.

> An ANTLR4 grammar for ECMAScript based on the Standard ECMA-262 5.1 Edition from June 2011.

## AST

The abstract syntax tree structure is based on [the ESTree Spec](https://github.com/estree/estree/blob/master/es5.md).

## Tests

Most of the tests were shamelessly stolen from [the Esprima repository](https://github.com/jquery/esprima/tree/master/test/fixtures).
If you are looking for a test demonstrating all supported language constructs, check out [this one](https://github.com/8symbols/pseudo-js-translator/blob/master/PseudoJsTranslatorTests/tests/ast-building/valid/all-in-one/0000.js).
