#!/bin/bash
if test "$OS" = "Windows_NT"
then
  MONO=""
else
  MONO="mono"
fi

$MONO .paket/paket.bootstrapper.exe
exit_code=$?
if [ $exit_code -ne 0 ]; then
  exit $exit_code
fi
$MONO .paket/paket.exe restore
exit_code=$?
if [ $exit_code -ne 0 ]; then
  exit $exit_code
fi
$MONO packages/FAKE/tools/FAKE.exe $@ --fsiargs build.fsx foo bar
