import 'dart:convert';

import 'package:admin/components/AppTools.dart';
import 'package:admin/screens/main/main_screen.dart';
import 'package:admin/screens/register/register_screen.dart';
import 'package:admin/utils/server_config.dart';
import 'package:flutter/material.dart';
import 'package:flutter/rendering.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:http/http.dart' as http;

int clientID = 0;
String token = "";

class LoginScreen extends StatelessWidget {
  LoginScreen({Key key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: ListView(
        padding: EdgeInsets.symmetric(
            horizontal: MediaQuery.of(context).size.width / 6),
        children: [Menu(), Body()],
      ),
    );
  }
}

class Menu extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 30),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.start,
            children: [
              _menuItem(title: 'Home'),
              _menuItem(title: 'About us'),
              // _menuItem(title: 'Contact us'),
              // _menuItem(title: 'Help'),
            ],
          ),
          Row(
            children: [
              _menuItem(title: 'Sign In', isActive: true),
              _menuItem(title: 'Register', isActive: false),
            ],
          ),
        ],
      ),
    );
  }

  Widget _menuItem({String title = 'Title Menu', isActive = false}) {
    return Padding(
      padding: const EdgeInsets.only(right: 75),
      child: MouseRegion(
        cursor: SystemMouseCursors.click,
        child: Column(
          children: [
            Text(
              '$title',
              style: TextStyle(
                fontWeight: FontWeight.bold,
                color: isActive ? Colors.deepPurple : Colors.grey,
              ),
            ),
            SizedBox(
              height: 6,
            ),
            isActive
                ? Container(
                    padding: EdgeInsets.symmetric(horizontal: 12, vertical: 2),
                    decoration: BoxDecoration(
                      color: Colors.deepPurple,
                      borderRadius: BorderRadius.circular(30),
                    ),
                  )
                : SizedBox()
          ],
        ),
      ),
    );
  }
}

class Body extends StatefulWidget {
  @override
  _BodyState createState() => _BodyState();
}

class _BodyState extends State<Body> {
  TextEditingController _email = new TextEditingController();
  TextEditingController _password = new TextEditingController();
  // UserRepository userRepository = new UserRepository();

  final scaffoldKey = new GlobalKey<ScaffoldState>();
  final snackBarKey = new GlobalKey<ScaffoldState>();

  Future saveLogin() async {
    SharedPreferences prefs = await SharedPreferences.getInstance();
    // md5 convert data behind sharepreferences
    prefs.setString('email', _email.text);
    prefs.setString('password', _password.text);
  }

  @override
  Widget build(BuildContext context) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        Container(
          width: 360,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Image.asset(
                'assets/images/logo_release.png',
                width: 300,
              ),
              Text(
                'ThinkMay Services',
                style: TextStyle(
                  color: Colors.black54,
                  fontSize: 45,
                  fontWeight: FontWeight.bold,
                ),
              ),
              SizedBox(
                height: 30,
              ),
              Text(
                "If you don't have an account",
                style: TextStyle(
                    color: Colors.black54, fontWeight: FontWeight.bold),
              ),
              SizedBox(
                height: 10,
              ),
              Row(
                children: [
                  Text(
                    "You can",
                    style: TextStyle(
                        color: Colors.black54, fontWeight: FontWeight.bold),
                  ),
                  SizedBox(width: 10),
                  GestureDetector(
                    onTap: () {
                      Navigator.of(context).push(
                        MaterialPageRoute(
                            builder: (context) => RegisterScreen()),
                      );
                    },
                    child: Text(
                      "Register!",
                      style: TextStyle(
                          color: Colors.deepPurple,
                          fontWeight: FontWeight.bold),
                    ),
                  ),
                ],
              ),
            ],
          ),
        ),
        Padding(
          padding: EdgeInsets.symmetric(
              vertical: MediaQuery.of(context).size.height / 6),
          child: Container(
            width: 320,
            child: _formLogin(),
          ),
        )
      ],
    );
  }

  Widget _formLogin() {
    return Column(
      children: [
        TextField(
          controller: _email,
          style: TextStyle(color: Colors.black54),
          decoration: InputDecoration(
            hoverColor: Colors.black12,
            hintText: 'Email',
            hintStyle: TextStyle(color: Colors.black38),
            filled: true,
            fillColor: Colors.blueGrey[100],
            labelStyle: TextStyle(fontSize: 12, color: Colors.black45),
            contentPadding: EdgeInsets.only(left: 30),
            enabledBorder: OutlineInputBorder(
              borderSide: BorderSide(color: Color(0xffEEEEEE)),
              borderRadius: BorderRadius.circular(15),
            ),
            focusedBorder: OutlineInputBorder(
              borderSide: BorderSide(color: Color(0xFFECEFF1)),
              borderRadius: BorderRadius.circular(15),
            ),
          ),
        ),
        SizedBox(height: 30),
        TextField(
          controller: _password,
          obscureText: true,
          style: TextStyle(color: Colors.black54),
          decoration: InputDecoration(
            hintText: 'Password',
            counterText: 'Forgot password?',
            counterStyle: TextStyle(color: Colors.black45),
            hintStyle: TextStyle(color: Colors.black38),
            hoverColor: Colors.black12,
            filled: true,
            fillColor: Colors.blueGrey[100],
            labelStyle: TextStyle(fontSize: 12),
            contentPadding: EdgeInsets.only(left: 30),
            enabledBorder: OutlineInputBorder(
              borderSide: BorderSide(color: Color(0xffECEFF1)),
              borderRadius: BorderRadius.circular(15),
            ),
            focusedBorder: OutlineInputBorder(
              borderSide: BorderSide(color: Color(0xffECEFF1)),
              borderRadius: BorderRadius.circular(15),
            ),
          ),
        ),
        SizedBox(height: 40),
        Container(
          decoration: BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.circular(30),
            boxShadow: [
              BoxShadow(
                color: Color(0xffD1C4E9),
                spreadRadius: 10,
                blurRadius: 20,
              ),
            ],
          ),
          child: ElevatedButton(
            child: Container(
                width: double.infinity,
                height: 50,
                child: Center(child: Text("Sign In"))),
            onPressed: verifyLogin,
            style: ElevatedButton.styleFrom(
              primary: Colors.deepPurple,
              onPrimary: Colors.white,
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(15),
              ),
            ),
          ),
        ),
        SizedBox(height: 40),
        Row(
          children: [
            Expanded(
              child: Divider(
                color: Colors.grey[300],
                height: 50,
              ),
            ),
          ],
        ),
      ],
    );
  }

  verifyLogin() async {
    if (_email.text == "") {
      showMaterialDialog(
          context, "Lỗi thông tin", "Vui lòng không để trống Email!", "OK");
      return;
    } else if (!_email.text.contains("@")) {
      showMaterialDialog(
          context, "Lỗi thông tin", "Email không hợp lệ !", "OK");
      return;
    } else if (_password.text == "") {
      showMaterialDialog(
          context, "Lỗi thông tin", "Vui lòng không để trống mật khẩu !", "OK");
      return;
    } else if (_password.text.length < 6) {
      showMaterialDialog(context, "Lỗi thông tin",
          "Mật khẩu quá ngắn. Vui lòng điền trên 6 ký tự.", "OK");
      return;
    } else {
      displayProgressDialog(context);
      // login services
      final response = await http.post(
        // Uri.parse('https://localhost:port/Admin/AddSlave'),
        Uri.parse('$urlServer/Account/Login'),
        headers: {
          'Content-type': 'application/json',
          'Accept': 'application/json',
          //  'Authorization': '<Your token>'
        },
        body: jsonEncode(<String, String>{
          'email': _email.text,
          'password': _password.text,
        }),
      );
      final Map parsed = json.decode(response.body);
      /*
      *** Successful
          ErrorCode = 0,
          UserEmail = email,
          Message = "Login successful",
          Token = token,
          ValidUntil = expiry,
          ClientID = _clientID,
       *** Failure
         ErrorCode = -1,
                Message = "Login failed",
                UserEmail = email 
      */
      print(parsed);
      clientID = parsed['clientID'];
      print(parsed['errorCode']);
      if (parsed['errorCode'] == 0) {
        print("Login Success");
        // saveLogin();
        closeProgressDialog(context);
        Navigator.of(context).pushAndRemoveUntil(
            MaterialPageRoute(builder: (context) => MainScreen(1)),
            (Route<dynamic> route) => false);
      } else {
        closeProgressDialog(context);
        showMaterialDialog(
            context, parsed['ErrorCode'], parsed['Message'], "OK");
      }
    }
  }
}
