#!/bin/bash

if cmp -s "$1" "$2"; then
    printf "$3"
else
    printf "$4"
fi