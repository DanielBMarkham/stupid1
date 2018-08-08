#!/bin/bash

rm -f TextSample1ToText.desiredout
rm -f TextSample1ToJson.desiredout
rm -f CGISample1ToText.desiredout
rm -f CGISample1ToJson.desiredout

cat TextSample1.testin | ./stupid1.exe -IF:T -OF:T > TextSample1ToText.desiredout
cat TextSample1.testin | ./stupid1.exe -IF:T -OF:J > TextSample1ToJson.desiredout
cat CGISample1.testin | ./stupid1.exe -IF:C -OF:T > CGISample1ToText.desiredout
cat CGISample1.testin | ./stupid1.exe -IF:C -OF:J > CGISample1ToJson.desiredout
