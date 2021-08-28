class Slave {
  final int ID;
  final String CPU;
  final String GPU;
  final int RAMcapacity;
  final String OS;
  final String CommandLogs;
  final String ServingSession;
  final String GeneralErrors;
  final String SessionCoreExits;

  final String Register;

  Slave({
    required this.ID,
    required this.CPU,
    required this.GPU,
    required this.RAMcapacity,
    required this.OS,
    required this.CommandLogs,
    required this.ServingSession,
    required this.GeneralErrors,
    required this.SessionCoreExits,
    required this.Register,
  });

  factory Slave.fromJson(Map<String, dynamic> json_outer) {
    var json = json_outer['Item1'];
    print(json['servingSession']);

    return Slave(
      ID: json['ID'],
      CPU: json['CPU'],
      GPU: json['GPU'],
      RAMcapacity: json['RAMcapacity'],
      OS: json['OS'],
      CommandLogs: "json['CommandLogs']" as String,
      ServingSession: "json['servingSession']" as String,
      GeneralErrors: "json['GeneralErrors']" as String,
      SessionCoreExits: "json['SessionCoreExits']" as String,
      Register: json['Register'] as String,
    );
  }
}
