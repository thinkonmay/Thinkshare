import 'dart:convert';

import 'package:flutter/material.dart';
import 'package:thinkmay_admin_client/screen/Dashboard/Dashboard.dart';
import 'package:thinkmay_admin_client/components/app_tools.dart';
import 'package:http/http.dart' as http;
import 'package:thinkmay_admin_client/screen/Register/Register.dart';

import '../../main.dart';

class Login extends StatefulWidget {
  @override
  _LoginState createState() => _LoginState();
}

class _LoginState extends State<Login> {
  TextEditingController _email = new TextEditingController();
  TextEditingController _password = new TextEditingController();
  bool isChecked = false;
  @override
  void initState() {
    super.initState();
  }

  @override
  Widget build(BuildContext context) {
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
          SizedBox(height: 10),
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: <Widget>[
              Row(
                children: <Widget>[
                  Checkbox(
                    value: isChecked,
                    onChanged: (value) {
                      setState(() {
                        isChecked = value!;
                      });
                    },
                  ),
                  Text("Nút này để cho đủ chỉ tiêu")
                ],
              ),
            ],
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
                  child: Center(child: Text("Log In"))),
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
          SizedBox(
            height: 20,
          ),
          GestureDetector(
              onTap: () {
                Navigator.push(context,
                    MaterialPageRoute(builder: (context) => Register()));
              },
              child: Text("Register",
                  style: TextStyle(
                    decoration: TextDecoration.underline,
                    fontSize: 14,
                  ))),
        ],
      );
    }

    return Scaffold(
      // backgroundColor: Colors.white,
      body: Container(
        decoration: BoxDecoration(
          image: DecorationImage(
            image: AssetImage("assets/images/bg.png"),
            fit: BoxFit.cover,
          ),
        ),
        child: Stack(
          children: <Widget>[
            Center(
              child: Container(
                width: MediaQuery.of(context).size.width / 2.5,
                height: MediaQuery.of(context).size.height / 1.5,
                child: Column(
                  children: <Widget>[
                    SizedBox(
                      height: 60,
                    ),
                    Center(
                      child: Text(
                        'Admin Hub',
                        style: TextStyle(fontSize: 32),
                      ),
                    ),
                    SizedBox(
                      height: 20,
                    ),
                    Expanded(
                      child: Container(
                        child: _formLogin(),
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  verifyLogin() async {
    if (_email.text == "") {
      showMaterialDialog(
          context: context,
          title: "Lỗi thông tin",
          content: "Vui lòng không để trống Email!",
          confirmText: "OK");
      return;
    } else if (!_email.text.contains("@")) {
      showMaterialDialog(
          context: context,
          title: "Lỗi thông tin",
          content: "Email không hợp lệ !",
          confirmText: "OK");
      return;
    } else if (_password.text == "") {
      showMaterialDialog(
          context: context,
          title: "Lỗi thông tin",
          content: "Vui lòng không để trống mật khẩu !",
          confirmText: "OK");
      return;
    } else if (_password.text.length < 6) {
      showMaterialDialog(
          context: context,
          title: "Lỗi thông tin",
          content: "Mật khẩu quá ngắn. Vui lòng điền trên 6 ký tự.",
          confirmText: "OK");
      return;
    } else {
      displayProgressDialog(context);
      // login services
      final response = await http.post(
        // Uri.parse('https://localhost:port/Admin/AddSlave'),
        Uri.parse('http://192.168.1.6:81/Account/Login'),
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
      print(response);
      final Map parsed = json.decode(response.body);
      print(parsed['errorCode']);
      if (parsed['errorCode'] == 0) {
        // print("Login Success");
        // saveLogin();
        closeProgressDialog(context);
        Navigator.of(context).pushAndRemoveUntil(
            MaterialPageRoute(
              builder: (context) =>
                  MyHomePage(title: 'Đang trong quá trình phát triển'),
            ),
            (Route<dynamic> route) => false);
      } else {
        closeProgressDialog(context);
        showMaterialDialog(
            context: context,
            title: "Đăng nhập thất bại",
            content:
                "Tài khoản đăng nhập không đúng! \nHoặc bạn chưa xác thực tài khoản của mình!",
            confirmText: "OK");
      }
    }
  }
}