@ECHO OFF
SET sn="C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\bin\sn.exe"
%sn% -p ETagMiddleware.snk ETagMiddleware.snk.PublicKey
%sn% -tp ETagMiddleware.snk.PublicKey > PublicToken.txt
PAUSE