import 'package:flutter/material.dart';
import 'package:thinkmay_admin_client/screen/Dashboard/Dashboard.dart';
import 'package:thinkmay_admin_client/screen/Drawer/Drawder.dart';
import 'package:thinkmay_admin_client/screen/NavigationBar/NavigationBar.dart';
import 'package:thinkmay_admin_client/screen/login/Login.dart';

void main() {
  runApp(MyWeb());
}

class MyWebApp extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return Container(
      child: Center(
        child: Text(
            "ThinkMay - Admin Hub"),
      ),
    );
  }
}

class MyWeb extends StatelessWidget {
  // This widget is the root of your application.
  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      debugShowCheckedModeBanner: false,
      title: 'ThinkMay - Admin Hub',
      theme: ThemeData(
        primarySwatch: Colors.blue,
        visualDensity: VisualDensity.adaptivePlatformDensity,
      ),
      home:Login(),
      // MyHomePage(title: 'Đang trong quá trình phát triển'),
    );
  }
}

class MyHomePage extends StatefulWidget {
  MyHomePage({Key? key, required this.title}) : super(key: key);

  final String title;

  @override
  _MyHomePageState createState() => _MyHomePageState();
}

class _MyHomePageState extends State<MyHomePage> {
  @override
  Widget build(BuildContext context) {
    return Scaffold(
        body: Container(
              height: MediaQuery.of(context).size.height,
              width: MediaQuery.of(context).size.width,
              child: 
              // ResponsiveLayout.isMacbook(context) ? 
              Stack(
                children: [
                  NavigationBar(),
                  Dashboard(),
                  Drawder(),
                ],
              ) 
              // : ListView(
              //   children: [
              //     NavigationBar(),
              //     Dashboard(),
              //   ],
              // )
        ));
  }
}