#!/bin/bash

rm -f TextSample1ToText.testout
rm -f TextSample1ToJson.testout
rm -f CGISample1ToText.testout
rm -f CGISample1ToJson.testout
rm -f JsonSample1ToText.testout
rm -f JsonSample1ToJson.testout
rm -f CGI2Text2Json1Sample.testout



cat TextSample1.testin | ./stupid1.exe -IF:T -OF:T > TextSample1ToText.testout
cat TextSample1.testin | ./stupid1.exe -IF:T -OF:J > TextSample1ToJson.testout
cat CGISample1.testin | ./stupid1.exe -IF:C -OF:T > CGISample1ToText.testout
cat CGISample1.testin | ./stupid1.exe -IF:C -OF:J > CGISample1ToJson.testout
cat JsonSample1.testin | ./stupid1.exe -IF:J -OF:T > JsonSample1ToText.testout
cat JsonSample1.testin | ./stupid1.exe -IF:J -OF:J > JsonSample1ToJson.testout

cat CGISample1.testin | ./stupid1.exe -IF:C -OF:T | ./stupid1.exe -IF:T -OF:J

cat CGISample1.testin | ./stupid1.exe -IF:C -OF:T > pipetemp1
cat pipetemp1 | ./stupid1.exe -IF:T -OF:J > CGI2Text2Json1Sample.testout
rm -f pipetemp1


./filecomp.sh CGISample1ToText.testout CGISample1ToText.desiredout "CGI1ToText Test: PASS\n" "CGI1ToText Test: FAIL\n"
./filecomp.sh CGISample1ToJson.testout CGISample1ToJson.desiredout "CGI1ToJson Test: PASS\n" "CGI1ToJson Test:: FAIL\n"
./filecomp.sh TextSample1ToText.testout TextSample1ToText.desiredout "Text1ToText Test: PASS\n" "Text1ToText Test: FAIL\n"
./filecomp.sh TextSample1ToJson.testout TextSample1ToJson.desiredout "Text1ToJson Test: PASS\n" "Text1ToJson Test: FAIL\n"
./filecomp.sh JsonSample1ToText.testout JsonSample1ToText.desiredout "Json1ToText Test: PASS\n" "Json1ToText Test: FAIL\n"
./filecomp.sh JsonSample1ToJson.testout JsonSample1ToJson.desiredout "JsonSample1ToJson Test: PASS\n" "JsonSample1ToJson Test: FAIL\n"

./filecomp.sh CGI2Text2Json1Sample.testout CGI2Text2Json1Sample.desiredout "CGI2Text2Json1Sample Test: PASS\n" "CGI2Text2Json1Sample Test: FAIL\n"


