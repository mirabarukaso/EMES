:A
rem @set CSC="C:\Windows\Microsoft.NET\Framework\v3.5\csc"
@set CSC=%windir%\Microsoft.NET\Framework\v4.0.30319\csc
@set MANAGED=C:\Games\Kiss\COM3D2\COM3D2x64_Data\Managed
@set BEPINEXCORE=C:\Games\Kiss\COM3D2\BepInEx\core
@set OPTS=/noconfig /optimize+ /nologo /nostdlib+ /utf8output /define:DEBUG /define:BEPINEX
@set OPTS=%OPTS% /t:library /lib:%MANAGED%,%BEPINEXCORE%,..\,..\..\ /r:UnityEngine.dll /r:Assembly-CSharp.dll /r:Assembly-CSharp-firstpass.dll /r:Assembly-UnityScript-firstpass.dll
@set OPTS=%OPTS% /r:Mono.Cecil.dll  
@set OPTS=%OPTS% /r:BepInEx.dll
@set OPTS=%OPTS% /r:%MANAGED%\mscorlib.dll
@set OPTS=%OPTS% /r:%MANAGED%\System.dll
@set OPTS=%OPTS% /r:%MANAGED%\System.Core.dll
@set OPTS=%OPTS% /r:%MANAGED%\System.Data.dll
@set OPTS=%OPTS% /r:%MANAGED%\System.Xml.dll
@set OPTS=%OPTS% /r:%MANAGED%\System.Xml.Linq.dll
@set OPTS=%OPTS% /r:%MANAGED%\System.Drawing.dll
@set OPTS=%OPTS% /r:%MANAGED%\System.Windows.Forms.dll

del *.dll

%CSC% %OPTS% /out:BepInEx.COM3D2.EnhancedMaidEditScene.Plugin.dll .\*.cs
@time /T
@pause
@goto A

