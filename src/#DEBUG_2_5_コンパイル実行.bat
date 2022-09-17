:A
rem @set CSC="C:\Windows\Microsoft.NET\Framework\v3.5\csc"
@set CSC=%windir%\Microsoft.NET\Framework\v4.0.30319\csc
@set MANAGED=C:\Games\Kiss\COM3D2_5\COM3D2x64_Data\Managed
@set OPTS=/noconfig /optimize+ /nologo /nostdlib+ /utf8output /define:DEBUG /define:COM3D25 /define:SYBARIS
@set OPTS=%OPTS% /t:library /lib:%MANAGED%,..\,..\..\ /r:UnityEngine.dll /r:Assembly-CSharp.dll /r:Assembly-CSharp-firstpass.dll /r:Mono.Cecil.dll  /r:JsonFx.Json.dll
@set OPTS=%OPTS% /r:UnityInjector.dll /r:ExIni.dll /r:"Assembly-UnityScript-firstpass.dll" 
@set OPTS=%OPTS% /r:"COM3D2.ExternalSaveData.Managed.dll"
@set OPTS=%OPTS% /r:%MANAGED%\mscorlib.dll
@set OPTS=%OPTS% /r:%MANAGED%\System.dll
@set OPTS=%OPTS% /r:%MANAGED%\System.Core.dll
@set OPTS=%OPTS% /r:%MANAGED%\System.Data.dll
@set OPTS=%OPTS% /r:%MANAGED%\System.Xml.dll
@set OPTS=%OPTS% /r:%MANAGED%\System.Xml.Linq.dll
@set OPTS=%OPTS% /r:%MANAGED%\System.Drawing.dll
@set OPTS=%OPTS% /r:%MANAGED%\System.Windows.Forms.dll

del *.dll
del *.cs
xcopy /y C:\Users\okm\source\repos\COM3D2.EnhancedMaidEditScene.Plugin\COM3D2.EnhancedMaidEditScene.Plugin\*.cs .

%CSC% %OPTS% /out:COM3D25.EnhancedMaidEditScene.Plugin.dll .\*.cs
@time /T
@pause
@goto A

