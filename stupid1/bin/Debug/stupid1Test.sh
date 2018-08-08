#!/bin/bash

rm -f TextSample1ToText.testout
rm -f TextSample1ToJson.testout
rm -f CGISample1ToText.testout
rm -f CGISample1ToJson.testout

cat TextSample1.testin | ./stupid1.exe -IF:T -OF:T > TextSample1ToText.testout
cat TextSample1.testin | ./stupid1.exe -IF:T -OF:J > TextSample1ToJson.testout
cat CGISample1.testin | ./stupid1.exe -IF:C -OF:T > CGISample1ToText.testout
cat CGISample1.testin | ./stupid1.exe -IF:C -OF:J > CGISample1ToJson.testout

./filecomp.sh CGISample1ToText.testout CGISample1ToText.desiredout "CGI1ToText Test: PASS\n" "CGI1ToText Test: FAIL\n"
./filecomp.sh CGISample1ToJson.testout CGISample1ToJson.desiredout "CGI1ToJson Test: PASS\n" "CGI1ToJson Test:: FAIL\n"
./filecomp.sh TextSample1ToText.testout TextSample1ToText.desiredout "Text1ToText Test: PASS\n" "Text1ToText Test: FAIL\n"
./filecomp.sh TextSample1ToJson.testout TextSample1ToJson.desiredout "Text1ToJson Test: PASS\n" "Text1ToJson Test: FAIL\n"
