ZKTeco SDK Installation Instructions
====================================

BEFORE RUNNING THE APPLICATION:

1. MUST RUN AS ADMINISTRATOR:
   - Right-click on "Install_ZKTeco.bat"
   - Select "Run as administrator"

2. REQUIRED DLLs:
   - zkemkeeper.dll (main COM component)
   - plcommpro.dll (required dependency)
   - comms.dll (required dependency)
   - All other DLLs in ZKTeco_SDK folder

3. VERIFICATION:
   After installation, you can test with:
   regsvr32 /s zkemkeeper.dll

4. TROUBLESHOOTING:
   - If "Access denied": Run Command Prompt as Admin
   - If "DLL not found": Check DLLs are in System32
   - If still not working: Restart the computer

5. SUPPORT:
   This application requires ZKTeco biometric device SDK
   Contact your system administrator if issues persist.