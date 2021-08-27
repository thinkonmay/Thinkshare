import 'package:signalr_netcore/hub_connection_builder.dart';
var urlServer = "http://125.212.237.45:81";


final hubConnection =
    HubConnectionBuilder().withUrl("https://125.212.237.45:81/ClientHub").build();
