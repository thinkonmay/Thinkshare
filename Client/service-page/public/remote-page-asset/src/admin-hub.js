
class AdminHub
{
  constructor()
  {
    this.onsessioncoreexit = this.ReportSessionCoreExit.bind(this);
    this.onsessioncoreerror = this.ReportSessionCoreError.bind(this);
    this.onagenterror = this.ReportAgentError.bind(this);
    this.onslaveregistered = this.ReportSlaveRegistered.bind(this);
    this.onsessionstart = this.ReportSessionStart.bind(this);
    this.onslavecommandline = this.LogSlaveCommandLine.bind(this);

    this.connection = null;
    this.onadmin = null;

    this.Connect();
  }


  Connect()
  {
    this.connection = new signalR.HubconnectionBuilder()
      .withUrl(ClientConfig.HostUrl+"/Admin")
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.connection.on('ReportSessionCoreExit', (slaveID, exit) => {  
      this.onsessioncoreexit(slaveID, exit) })
    this.connection.on('ReportSessionCoreError', (error) => { 
      this.onsessioncoreerror(error) })
    this.connection.on('ReportAgentError', (error) => {  
      this.onagenterror(error) })
    this.connection.on('ReportSlaveRegistered', (information) => {  
      this.onslaveregistered(information) })
    this.connection.on('ReportSessionStart', (SlaveID, ClientID) => { 
      this.onsessionstart(SlaveID,ClientID) })
    this.connection.on('LogSlaveCommandLine', (SlaveID, result) => { 
      this.onslavecommandline(SlaveID,ProcessID, Command) })
  }

  ReportSessionCoreExit(slaveID, exit)
  {
    this.onadmin(`[SlaveID ${slaveID}] : session core exited with code ${exit.ExitCode} during\n${exit.CoreState}\n${exit.PipelineState}\n${exit.PeerCallState}\n${exit.Message}`);
  }
  ReportSessionCoreError(error)
  {
    this.onadmin(`[SlaveID ${error.Id}] : session core error ${error.ErrorMessage}`);
  }

  ReportAgentError(error)
  {
    this.onadmin(`[SlaveID ${error.Id}] : agent error ${error.ErrorMessage}`);

  }

  ReportSlaveRegistered(information)
  {
    this.onadmin(`[SlaveID ${information.Id}] : registered, CPU ${information.CPU}, RAM capacity: ${information.RAMcapacity}, GPU ${information.GPU}}`);
  }

  ReportSessionStart(SlaveID, ClientID)
  {
    this.onadmin(`[SlaveID ${SlaveID}] : session start with user ${ClientID}`);
  }

  LogSlaveCommandLine(SlaveID,ProcessID, Command)
  {
    this.onadmin(`[SlaveID ${SlaveID}] [ProcessID ${ProcessID}] : Commandline output : ${Command}`);
  }

}
