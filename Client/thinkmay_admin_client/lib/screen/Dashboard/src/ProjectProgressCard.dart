import 'package:flutter/material.dart';
import 'package:flutter_vector_icons/flutter_vector_icons.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:thinkmay_admin_client/components/app_tools.dart';

class ProjectProgressCard extends StatefulWidget {
  final Color color;
  final Color? progressIndicatorColor;
  final String slaveName;
  final bool state;
  final int id;
  final String CPU;
  final String GPU;
  final int RAMcapacity;
  final String OS;
  final String CommandLogs;
  final String ServingSession;
  final String GeneralErrors;
  final String SessionCoreExits;
  final IconData icon;
  final String date;
  ProjectProgressCard({
    required this.color,
    required this.progressIndicatorColor,
    required this.slaveName,
    required this.state,
    required this.id,
    required this.CPU,
    required this.GPU,
    required this.RAMcapacity,
    required this.OS,
    required this.CommandLogs,
    required this.ServingSession,
    required this.GeneralErrors,
    required this.SessionCoreExits,
    required this.icon,
    required this.date,
  });
  @override
  _ProjectProgressCardState createState() => _ProjectProgressCardState();
}

class _ProjectProgressCardState extends State<ProjectProgressCard> {
  bool hovered = false;
  @override
  Widget build(BuildContext context) {
    return MouseRegion(
      onEnter: (value) {
        setState(() {
          hovered = true;
        });
      },
      onExit: (value) {
        setState(() {
          hovered = false;
        });
      },
      child: AnimatedContainer(
        duration: Duration(milliseconds: 275),
        height: hovered ? 225.0 : 220.0,
        width: hovered ? 305.0 : 300.0,
        decoration: BoxDecoration(
            color: hovered ? widget.color : Colors.white,
            borderRadius: BorderRadius.circular(15.0),
            boxShadow: [
              BoxShadow(
                color: Colors.black12,
                blurRadius: 20.0,
                spreadRadius: 5.0,
              ),
            ]),
        child: Center(
          child: Column(
            children: [
              SizedBox(
                height: 10.0,
              ),
              Row(
                mainAxisAlignment: MainAxisAlignment.end,
                children: <Widget>[
                  Expanded(
                    child: Row(
                      mainAxisAlignment: MainAxisAlignment.start,
                      children: [
                        Container(
                          margin: EdgeInsets.only(left: 32.0),
                          height: 30.0,
                          width: 30.0,
                          child: Icon(
                            widget.icon,
                            color: !hovered ? Colors.white : Colors.black,
                            size: 16.0,
                          ),
                          decoration: BoxDecoration(
                            borderRadius: BorderRadius.circular(30.0),
                            color: hovered ? Colors.white : Colors.black,
                          ),
                        ),
                        Container(
                          margin: EdgeInsets.only(
                            left: 13.0,
                          ),
                          child: Text(
                            widget.slaveName,
                            style: GoogleFonts.quicksand(
                              fontWeight: FontWeight.w500,
                              fontSize: 14.0,
                              color: hovered ? Colors.white : Colors.black,
                            ),
                          ),
                        ),
                      ],
                    ),
                  ),
                  InkWell(
                    onTap: () {
                      showDialog(
                          context: context,
                          builder: (_) => AlertDialog(
                                title: Text("Info ${widget.slaveName}"),
                                content: Container(
                                    child: Column(
                                      children: <Widget>[
                                        ListTile(
                                          title: Text("CommandLogs"),
                                          subtitle: Text(widget.CommandLogs),
                                          trailing: Container(
                                            height: 30.0,
                                            width: 30.0,
                                            child: Icon(
                                              Icons.delete,
                                              size: 16.0,
                                            ),
                                            decoration: BoxDecoration(
                                              borderRadius:
                                                  BorderRadius.circular(30.0),
                                            ),
                                          ),
                                        ),
                                        ListTile(
                                          title: Text("ServingSession"),
                                          subtitle: Text(widget.ServingSession),
                                          trailing: Container(
                                            height: 30.0,
                                            width: 30.0,
                                            child: Icon(
                                              Icons.delete,
                                              size: 16.0,
                                            ),
                                            decoration: BoxDecoration(
                                              borderRadius:
                                                  BorderRadius.circular(30.0),
                                            ),
                                          ),
                                        ),
                                        ListTile(
                                          title: Text("GeneralErrors"),
                                          subtitle: Text(widget.GeneralErrors),
                                          trailing: Container(
                                            height: 30.0,
                                            width: 30.0,
                                            child: Icon(
                                              Icons.delete,
                                              size: 16.0,
                                            ),
                                            decoration: BoxDecoration(
                                              borderRadius:
                                                  BorderRadius.circular(30.0),
                                            ),
                                          ),
                                        ),
                                        ListTile(
                                          title: Text("SessionCoreExits"),
                                          subtitle:
                                              Text(widget.SessionCoreExits),
                                          trailing: Container(
                                            height: 30.0,
                                            width: 30.0,
                                            child: Icon(
                                              Icons.delete,
                                              size: 16.0,
                                            ),
                                            decoration: BoxDecoration(
                                              borderRadius:
                                                  BorderRadius.circular(30.0),
                                            ),
                                          ),
                                        ),
                                      ],
                                    ),
                                  ),
                                actions: <Widget>[
                                  ElevatedButton(
                                    child: Text("Exit"),
                                    onPressed: () {
                                      Navigator.of(context).pop();
                                    },
                                  )
                                ],
                              ));
                    },
                    child: Container(
                      margin: EdgeInsets.only(right: 18.0),
                      height: 30.0,
                      width: 30.0,
                      child: Icon(
                        Icons.info,
                        color: !hovered ? Colors.white : Colors.black,
                        size: 16.0,
                      ),
                      decoration: BoxDecoration(
                        borderRadius: BorderRadius.circular(30.0),
                        color: hovered ? Colors.white : Colors.black,
                      ),
                    ),
                  ),
                ],
              ),
              SizedBox(
                height: 5.0,
              ),
              Row(
                mainAxisAlignment: MainAxisAlignment.end,
                children: <Widget>[
                  Expanded(
                    child: Row(
                      mainAxisAlignment: MainAxisAlignment.start,
                      children: [
                        Container(
                          margin: EdgeInsets.only(top: 8.0, left: 18.0),
                          child: Text(
                            "State",
                            style: GoogleFonts.quicksand(
                              fontWeight: FontWeight.w500,
                              fontSize: 12.5,
                              color: hovered ? Colors.white : Colors.black,
                            ),
                          ),
                        ),
                      ],
                    ),
                  ),
                  Container(
                    margin: EdgeInsets.only(top: 8.0, right: 32.0),
                    child: Text(
                      widget.state ? "ON" : "OFF",
                      style: GoogleFonts.quicksand(
                        fontWeight: FontWeight.w500,
                        fontSize: 12.5,
                        color: hovered ? Colors.white : Colors.black,
                      ),
                    ),
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
                        Container(
                          margin: EdgeInsets.only(top: 8.0, left: 18.0),
                          child: Text(
                            "CPU",
                            style: GoogleFonts.quicksand(
                              fontWeight: FontWeight.w500,
                              fontSize: 12.5,
                              color: hovered ? Colors.white : Colors.black,
                            ),
                          ),
                        ),
                      ],
                    ),
                  ),
                  Container(
                    width: 150.0,
                    margin: EdgeInsets.only(top: 8.0, right: 18.0),
                    child: Text(
                      widget.CPU,
                      // overflow: TextOverflow.,
                      maxLines: 2,
                      style: GoogleFonts.quicksand(
                        fontWeight: FontWeight.w500,
                        fontSize: 12.5,
                        color: hovered ? Colors.white : Colors.black,
                      ),
                    ),
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
                        Container(
                          margin: EdgeInsets.only(top: 8.0, left: 18.0),
                          child: Text(
                            "GPU",
                            style: GoogleFonts.quicksand(
                              fontWeight: FontWeight.w500,
                              fontSize: 12.5,
                              color: hovered ? Colors.white : Colors.black,
                            ),
                          ),
                        ),
                      ],
                    ),
                  ),
                  Container(
                    margin: EdgeInsets.only(top: 8.0, right: 18.0),
                    child: Text(
                      widget.GPU,
                      style: GoogleFonts.quicksand(
                        fontWeight: FontWeight.w500,
                        fontSize: 12.5,
                        color: hovered ? Colors.white : Colors.black,
                      ),
                    ),
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
                        Container(
                          margin: EdgeInsets.only(top: 8.0, left: 18.0),
                          child: Text(
                            "Ram",
                            style: GoogleFonts.quicksand(
                              fontWeight: FontWeight.w500,
                              fontSize: 12.5,
                              color: hovered ? Colors.white : Colors.black,
                            ),
                          ),
                        ),
                      ],
                    ),
                  ),
                  Container(
                    margin: EdgeInsets.only(top: 8.0, right: 18.0),
                    child: Text(
                      "${widget.RAMcapacity} GB",
                      style: GoogleFonts.quicksand(
                        fontWeight: FontWeight.w500,
                        fontSize: 12.5,
                        color: hovered ? Colors.white : Colors.black,
                      ),
                    ),
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
                        Container(
                          margin: EdgeInsets.only(top: 8.0, left: 18.0),
                          child: Text(
                            "OS",
                            style: GoogleFonts.quicksand(
                              fontWeight: FontWeight.w500,
                              fontSize: 12.5,
                              color: hovered ? Colors.white : Colors.black,
                            ),
                          ),
                        ),
                      ],
                    ),
                  ),
                  Container(
                    margin: EdgeInsets.only(top: 8.0, right: 18.0),
                    child: Text(
                      widget.OS,
                      style: GoogleFonts.quicksand(
                        fontWeight: FontWeight.w500,
                        fontSize: 12.5,
                        color: hovered ? Colors.white : Colors.black,
                      ),
                    ),
                  ),
                ],
              ),
              SizedBox(
                height: 8.0,
              ),
              Row(
                mainAxisAlignment: MainAxisAlignment.end,
                children: <Widget>[
                  Expanded(
                    child: Row(
                      mainAxisAlignment: MainAxisAlignment.start,
                      children: [
                        Padding(
                          padding: EdgeInsets.only(left: 18.0),
                          child: new RaisedButton(
                            color: Colors.greenAccent,
                            shape: new RoundedRectangleBorder(
                                borderRadius: new BorderRadius.all(
                                    new Radius.circular(15.0))),
                            onPressed: () {},
                            child: Container(
                              height: 30.0,
                              width: 80.0,
                              padding: const EdgeInsets.all(8.0),
                              child: Center(
                                child: new Text(
                                  "Connect",
                                  style: TextStyle(
                                      color: Colors.black, fontSize: 12.5),
                                ),
                              ),
                            ),
                          ),
                        ),
                      ],
                    ),
                  ),
                  Padding(
                    padding: EdgeInsets.only(right: 18.0),
                    child: new RaisedButton(
                      color: Colors.redAccent,
                      shape: new RoundedRectangleBorder(
                          borderRadius:
                              new BorderRadius.all(new Radius.circular(15.0))),
                      onPressed: () {},
                      child: Container(
                        height: 30.0,
                        width: 80.0,
                        padding: const EdgeInsets.all(8.0),
                        child: Center(
                          child: new Text(
                            "Reject",
                            style:
                                TextStyle(color: Colors.white, fontSize: 12.5),
                          ),
                        ),
                      ),
                    ),
                  ),
                ],
              ),
              // AnimatedContainer(
              //   duration: Duration(milliseconds: 275),
              //   margin: EdgeInsets.only(top: 5.0),
              //   height: 6.0,
              //   width: 160.0,
              //   decoration: BoxDecoration(
              //     color: hovered
              //         ? widget.progressIndicatorColor
              //         : Color(0xffF5F6FA),
              //     borderRadius: BorderRadius.circular(20.0),
              //   ),
              //   child: Align(
              //     alignment: Alignment.centerLeft,
              //     child: AnimatedContainer(
              //       duration: Duration(milliseconds: 275),
              //       height: 6.0,
              //       width:
              //           (double.parse(widget.percentComplete.substring(0, 1)) /
              //                   10) *
              //               250.0,
              //       decoration: BoxDecoration(
              //         color: hovered ? Colors.white : widget.color,
              //         borderRadius: BorderRadius.circular(20.0),
              //       ),
              //     ),
              //   ),
              // ),
            ],
          ),
        ),
      ),
    );
  }
}
