class ServingSession {
  final int ClientID;
  final int SlaveID;
  final int SessionSlaveID;
  final int SessionClientID;
  final String StartTime;
  final String EndTime;
  final String SignallingUrl;
  final String StunServer;
  final bool ClientOffer;
  final String QoE;

  ServingSession(
      {this.ClientID,
      this.SlaveID,
      this.SessionSlaveID,
      this.SessionClientID,
      this.StartTime,
      this.EndTime,
      this.SignallingUrl,
      this.StunServer,
      this.ClientOffer,
      this.QoE});

  factory ServingSession.fromJson(Map<String, dynamic> json) {
    return ServingSession(
        ClientID: json['ClientID'],
        SlaveID: json['SlaveID'],
        SessionSlaveID: json['SessionSlaveID'],
        SessionClientID: json['SessionClientID'],
        StartTime: json['StartTime'],
        EndTime: json['EndTime'],
        SignallingUrl: json['SignallingUrl'],
        StunServer: json['StunServer'],
        ClientOffer: json['ClientOffer'],
        QoE: json['QoE']);
  }

  /*
  "ClientID": 0,
"SlaveID": 21263759,
"SessionSlaveID": 1586729097,
"SessionClientID": 398976275,
"StartTime": "2021-08-08T13:46:50.1733333",
"EndTime": null,
"SignallingUrl": "http://125.212.237.45:80",
"StunServer": "stun://stun.l.google.com:19302",
"ClientOffer": false,
"QoE": null
  */
}
