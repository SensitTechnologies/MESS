MESS uses **Serilog** for its logging. The out of box setup for MESS involves the following log locations and levels.
All logs are written to the **Console** as well as the **Files** themselves.

There are 3 types of log files:
1. **Information** logs are stored within a daily log file as plaintext. The filename follows this format: *MESS_Blazor_All[DATE].logs*
2. **Warning** logs are stored in a separate file that has a rolling interval of 1 Day. The filename follows the format: *MESS_Blazor_Warning[DATE].logs*
3. **Error** logs are stored in a separate file that has a rolling interval of 1 Month. The filename follows the format: *MESS_Blazor_Error[DATE].logs*


### Log Locations
Logs are stored within the *MESS.Blazor* project within the directory **/Logs**

### Production Specific
The minimum log level within development is **Debug** whereas in Production the minimum log level is **Warning**.