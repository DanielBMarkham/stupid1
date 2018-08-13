#!/bin/bash

cat CGISample1.testin | ./stupid1.exe -IF:C -OF:T > pipetemp1
cat pipetemp1 | ./stupid1.exe -IF:T -OF:J > CGI2Text2Json1Sample.desiredout
rm -f pipetemp1
