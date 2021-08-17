import 'package:flutter/material.dart';

import 'ProgressDialog.dart';

showMaterialDialog(
    {required BuildContext context,
    required String title,
    required String content,
    required String confirmText}) {
  showDialog(
      context: context,
      builder: (_) => new AlertDialog(
            title: new Text(title),
            content: new Text(content),
            actions: <Widget>[
              ElevatedButton(
                child: Text(confirmText),
                onPressed: () {
                  Navigator.of(context).pop();
                },
              )
            ],
          ));
}

displayProgressDialog(BuildContext context) {
  Navigator.of(context).push(new PageRouteBuilder(
      opaque: false,
      pageBuilder: (BuildContext context, _, __) {
        return new ProgressDialog();
      }));
}

closeProgressDialog(BuildContext context) {
  Navigator.of(context).pop();
}

showSnackbar(String message, final scaffoldKey) {
  scaffoldKey.currentState.showSnackBar(new SnackBar(
      duration: Duration(seconds: 2),
      backgroundColor: Color(0xff60baed),
      content: new Text(
        message,
        style: new TextStyle(color: Colors.white),
        textAlign: TextAlign.center,
      )));
}
