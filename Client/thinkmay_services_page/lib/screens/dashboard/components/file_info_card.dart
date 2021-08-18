import 'dart:convert';

import 'package:admin/components/AppTools.dart';
import 'package:admin/models/MyFiles.dart';
import 'package:admin/models/ServingSession.dart';
import 'package:admin/models/Slave.dart';
import 'package:admin/screens/login/login_screen.dart';
import 'package:admin/services/api_services.dart';
import 'package:admin/utils/server_config.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:flutter_svg/flutter_svg.dart';
import 'dart:html' as html;
import '../../../constants.dart';

class FileInfoCard extends StatefulWidget {
  const FileInfoCard({
    Key? key,
    required this.slave,
  }) : super(key: key);

  final Slave slave;

  @override
  _FileInfoCardState createState() => _FileInfoCardState();
}

class _FileInfoCardState extends State<FileInfoCard> {
  @override
  void initState() {
    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: EdgeInsets.all(16.0),
      decoration: BoxDecoration(
        color: secondaryColor,
        borderRadius: const BorderRadius.all(Radius.circular(8)),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Container(
                padding: EdgeInsets.all(defaultPadding * 0.75),
                height: 40,
                width: 40,
                decoration: BoxDecoration(
                  color: primaryColor.withOpacity(0.1),
                  borderRadius: const BorderRadius.all(Radius.circular(10)),
                ),
                child: SvgPicture.asset(
                  "assets/icons/computer.svg",
                  width: 40,
                  height: 40,
                  // color: info.color,
                ),
              ),
              Text("Device 1",
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: TextStyle(color: Colors.black)),
              GestureDetector(
                child: Icon(Icons.more_vert, color: Colors.black54),
                onTap: () {
                  showMaterialDialog(context, "IDk", "IDK", "OK");
                },
              )
            ],
          ),
          // ProgressLine(
          //   color: info.color,
          //   percentage: info.percentage,
          // ),
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(
                "RAM:",
                style: Theme.of(context)
                    .textTheme
                    .caption!
                    .copyWith(color: Colors.black45),
              ),
              Text(
                "${widget.slave.RAMcapacity} GB",
                style: Theme.of(context)
                    .textTheme
                    .caption!
                    .copyWith(color: Colors.black),
              ),
            ],
          ),
          Row(
            mainAxisAlignment: MainAxisAlignment.end,
            children: <Widget>[
              Expanded(
                child: Row(
                  mainAxisAlignment: MainAxisAlignment.start,
                  children: [
                    Text(
                      "CPU:",
                      style: Theme.of(context)
                          .textTheme
                          .caption!
                          .copyWith(color: Colors.black45),
                    ),
                  ],
                ),
              ),
              Container(
                width: 150.0,
                margin: EdgeInsets.only(right: 5.0),
                child: Text(
                  "${widget.slave.CPU}",
                  maxLines: 2,
                  style: Theme.of(context)
                      .textTheme
                      .caption!
                      .copyWith(color: Colors.black),
                ),
              ),
            ],
          ),
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(
                "GPU:",
                style: Theme.of(context)
                    .textTheme
                    .caption!
                    .copyWith(color: Colors.black45),
              ),
              Text(
                "${widget.slave.GPU}",
                maxLines: 3,
                style: Theme.of(context)
                    .textTheme
                    .caption!
                    .copyWith(color: Colors.black),
              ),
            ],
          ),
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(
                "OS:",
                style: Theme.of(context)
                    .textTheme
                    .caption!
                    .copyWith(color: Colors.black45),
              ),
              Text(
                "${widget.slave.OS}",
                maxLines: 3,
                style: Theme.of(context)
                    .textTheme
                    .caption!
                    .copyWith(color: Colors.black),
              ),
            ],
          ),
          Expanded(
            child: Center(
              child: Padding(
                padding: EdgeInsets.only(top: 5.0),
                child: new RaisedButton(
                  color: Colors.greenAccent,
                  shape: new RoundedRectangleBorder(
                      borderRadius:
                          new BorderRadius.all(new Radius.circular(15.0))),
                  onPressed: () async {
                    html.window.open('''
http://192.168.1.6:81/Session/Initialize?
ClientId=5935953&
SlaveId=123345&
ScreenWidth=${MediaQuery.of(context).size.width.toInt()}&
ScreenHeight=${MediaQuery.of(context).size.height.toInt()}&
bitrate=1000000&
QoEMode=0&
VideoCodec=1&
AudioCodec=3
''', "Remote Page");
                  },
                  child: Container(
                    // height: 5.0,
                    width: 80.0,
                    padding: const EdgeInsets.all(8.0),
                    child: Center(
                      child: Text(
                        "Connect",
                        style: TextStyle(color: Colors.black, fontSize: 12),
                      ),
                    ),
                  ),
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class ProgressLine extends StatelessWidget {
  const ProgressLine({
    Key? key,
    this.color = primaryColor,
    required this.percentage,
  }) : super(key: key);

  final Color? color;
  final int? percentage;

  @override
  Widget build(BuildContext context) {
    return Stack(
      children: [
        Container(
          width: double.infinity,
          height: 5,
          decoration: BoxDecoration(
            color: color!.withOpacity(0.1),
            borderRadius: BorderRadius.all(Radius.circular(10)),
          ),
        ),
        LayoutBuilder(
          builder: (context, constraints) => Container(
            width: constraints.maxWidth * (percentage! / 100),
            height: 5,
            decoration: BoxDecoration(
              color: color,
              borderRadius: BorderRadius.all(Radius.circular(10)),
            ),
          ),
        ),
      ],
    );
  }
}
