import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:thinkmay_admin_client/components/responsive_layout.dart';


class SharedFilesItem extends StatefulWidget {
  final String sharedFileName;
  final Color color;
  final String members;
  final String et;
  final String fileSize;

  SharedFilesItem({
    required this.color,
    required this.et,
    required this.fileSize,
    required this.members,
    required this.sharedFileName,
  });

  @override
  _SharedFilesItemState createState() => _SharedFilesItemState();
}

class _SharedFilesItemState extends State<SharedFilesItem> {
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
        margin: EdgeInsets.only(bottom: 10.0, left: 5.0, right: 5.0),
        padding: EdgeInsets.all(10.0),
        decoration: BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.circular(10.0),
            boxShadow: hovered
                ? [
                    BoxShadow(
                      color: Colors.black12,
                      blurRadius: 13.0,
                      spreadRadius: 0.0,
                    ),
                  ]
                : []),
        child: Column(
          children: [
            Container(
              child: Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Row(
                    children: [
                      SizedBox(
                        width: ResponsiveLayout.isMacbook(context) ? 15.0 : 0,
                      ),
                      Container(
                        height: 28.0,
                        width: 28.0,
                        decoration: BoxDecoration(
                          color: widget.color.withOpacity(0.2),
                          borderRadius: BorderRadius.circular(5.0),
                        ),
                        child: Center(
                          child: Icon(
                            Icons.folder,
                            color: widget.color,
                            size: 15.0,
                          ),
                        ),
                      ),
                      SizedBox(
                        width: ResponsiveLayout.isMacbook(context) ? 15.0 : 5.0,
                      ),
                      Text(
                        widget.sharedFileName,
                        style: GoogleFonts.quicksand(
                          fontWeight: FontWeight.bold,
                          fontSize: 12.0,
                        ),
                      ),
                    ],
                  ),
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceAround,
                    children: [
                      Padding(
                        padding: EdgeInsets.symmetric(horizontal:  ResponsiveLayout.isMacbook(context) ? 30.0: 2.0),
                        child: 
                        
                        Row(
                          children: [
                            Text(
                          widget.members + " ",
                          style: GoogleFonts.quicksand(
                            fontWeight: FontWeight.bold,
                            fontSize: 11.0,
                            color: Colors.black45,
                          ),
                        ),
                            Icon(
                              Icons.group,
                            ),
                          ],
                        )
                        
                      ),
                      Padding(
                        padding: EdgeInsets.symmetric(horizontal:  ResponsiveLayout.isMacbook(context) ? 30.0: 2.0),
                        child: Text(
                          widget.et,
                          style: GoogleFonts.quicksand(
                            fontWeight: FontWeight.bold,
                            fontSize: 11.0,
                            color: Colors.black45,
                          ),
                        ),
                      ),
                      Padding(
                        padding: EdgeInsets.symmetric(horizontal:  ResponsiveLayout.isMacbook(context) ? 30.0: 2.0 ),
                        child: Text(
                          widget.fileSize,
                          style: GoogleFonts.quicksand(
                            fontWeight: FontWeight.bold,
                            fontSize: 11.0,
                            color: Colors.black87,
                          ),
                        ),
                      ),
                    ],
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}