import 'package:signalr_netcore/hub_connection_builder.dart';
var urlServer = "http://192.168.1.6:81";


final hubConnection =
    HubConnectionBuilder().withUrl("https://192.168.1.6:81/ClientHub").build();
