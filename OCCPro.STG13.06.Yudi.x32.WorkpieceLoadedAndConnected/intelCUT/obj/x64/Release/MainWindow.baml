   M S B A M L   `   `   `  ���� B  >WPF.MDI, Version=1.1.0.0, Culture=neutral, PublicKeyToken=null2&clr-namespace:WPF.MDI;assembly=WPF.MDIWPF.MDI  O Kmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e0891&clr-namespace:System;assembly=mscorlibSystem  intelCUT,   &OnlineCuttingControlProcess.MainWindow   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35U QWPFToolkit, Version=3.5.40128.1, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     6mdi&clr-namespace:WPF.MDI;assembly=WPF.MDI     9System&clr-namespace:System;assembly=mscorlib     -     � Title$�  intelCUT - Intelligent Online Cutting Control (OCC) Process on Computer Numerical Control (CNC) Machining ©      Version 01.00��$	��900��$
��1416�� � Icon$ CNCMach.ico��
������� � WindowStartupLocation$ CenterScreen=� � 
ResizeMode$ CanMinimize=�.���  J� RenderTransformOrigin$ 0.463,0.303&�$��0,0,0,-5q�.+�o� -    o�Name$ mainMenu��$��27��$	��Top=�$��0,4,0,0q�$��2��.@�m� -    m�Name$ 
configMenu��$��Configuration��.@�m� -   $ CNCTypeMenuItem��$��CNC Type�� m� Icon �� $��CNCmach.ico��.@�m� -   $��
HEIDENHAIN��m� -   $��FANUC��m� -   $��SIEMENS��m� -   $��Not Specified��m� -	   $ ConnectionMenuItem��$��
Connection�� �� $��connect.ico��.@�m� -
   $��ON��m� -   $	��OFF��m� -   $ InputDataMenuItem��$��Simulated Data�� �� $��data.ico��.@�m� -   $
��LOAD��m� -   $��UNLOAD��m� -   $��Tool Geometry�� �� $��milling2.ico��m� $��Options��.@�m� $��Workpiece Geometry��.@�m� -   $
��LOAD��m� -   $��UNLOAD��m� $��Data Recording��.@�m� -   $��ON��m� -   $	��OFF��m� $��Kalman Filtering��.@�m� $��
Position X��.@�m� -   $��ACTIVATE/Re-Open��m� -   $��
DEACTIVATE��m� $��
Position Y��.@�m� -   $��ACTIVATE/Re-Open��m� -   $��
DEACTIVATE��m� $��
Position Z��.@�m� -   $��ACTIVATE/Re-Open��m� -   $��
DEACTIVATE��m� -   $��DEACTIVATE ALL��m� $��	TEST MODE��.@�m� -   $��ON-STANDALONE��m� -   $��ON-DEPENDENT��m� -   $	��OFF��m� -   $��Customization��m� $
��View��.@�m� -   $ DataRecordGraphSubMenu��$��Graphic Records��.@�m� $��Single��.@�m� -    $��Fa��m� -!   $
��eta1��m� -"   $
��eta2��m� -#   $��FeedR��m� -$   $��Force��m� -%   $��FeedR[U]��m� -&   $��Force[Y]��m� -'   $��Kt-Upd��m� -(   $��F-Sim��m� -)   $��F-Mean��m� $��Compare��.@�m� -*   $��Fa��m� -+   $
��eta1��m� -,   $
��eta2��m� $��FeedR��.@�m� --   $��FeedR��m� -.   $��FeedR[U]��m� $��Force��.@�m� -/   $��Force��m� -0   $��Force[Y]��m� -1   $��F-Sim��m� -2   $��F-Mean��m� $��FeedR[U]��.@�m� -3   $��FeedR��m� -4   $��FeedR[U]��m� $��Force[Y]��.@�m� -5   $��Force��m� -6   $��Force[Y]��m� -7   $��F-Sim��m� -8   $��F-Mean��m� -9   $��Kt-Upd��m� $��F-Sim��.@�m� -:   $��Force��m� -;   $��Force[Y]��m� -<   $��F-Sim��m� -=   $��F-Mean��m� $��F-Mean��.@�m� ->   $��Force��m� -?   $��Force[Y]��m� -@   $��F-Sim��m� -A   $��F-Mean��m� -B   $ DataRecordTextSubMenu��$��Data Records��.@�m� -C   $
��Main��m� -D   $��Fa��m� -E   $
��eta1��m� -F   $
��eta2��m� -G   $��FeedR��m� -H   $��Force��m� -I   $��FeedR[U]��m� -J   $��Force[Y]��m� -K   $��Kt-Upd��m� -L   $��F-Sim��m� -M   $��F-Mean��m� $��Online Graphics��.@�m� -N   $��Force vs. Distance��m� -O   $!��Spindle Torque vs. Distance��m� $��Feed Rate vs. Distance��m� $��X Cutting Force vs. Time��m� $	��Run��.@�m� -P   $��Start��m� -Q   $
��Stop��m� $
��Help��.@�m� -R   $��Dependencies��m� -S   $��Prerequsites��m� -T   $��Connection Setup��m� -U   $��
User Guide��m� -V   $��
Contact Us���� $��14���� $��Simulated Data File:��$
��Bold"��� -W    ��Name$ inputDataFileName��$<��6                                                      ��$��20��$	��540�����	#FFF9FAFA$��0���� -X   	 ��Name$	 tracingStatusButton��$��NOT STARTED��$	��144��$
��Bold"��� -Y   $	 processTimeButton��$��PROCESS TIME��$
��Bold"�$	��120���� -Z   $	 alarmStatus��$��NO ALARM��$	��130��$
��Bold"��� -[   
 ��Name$
 configPanel��
���   �$��1q�$
��Left=�$	��263��$��4,34,0,0q�$	��Top=�$	��167��._��� $
��Left=�$��47��$��2,22,0,0q� �� TextWrapping$
 Wrap=�$��                CNC Type��$	��Top=�$	��161���� -\   $	 cncTypeButton��$��NOT SPECIFIED��$
��Left=�$��22��$��	13,78,0,0q�$	��Top=�$	��149��$
��Bold"��� $
��Left=�$��20��$��5,35,0,0q�$
 Wrap=�$��         CONFIGURATION��$	��Top=�$	��165��$
��Bold"����	#FFFFA200���	#FFFEFEFE�� $
��Left=�$��47��$��	7,106,0,0q�$
 Wrap=�$��              Connection��$	��Top=�$	��161���� -]   $	 connectionCncButton��$	��OFF��$
��Left=�$��22��$��
13,127,0,0q�$	��Top=�$	��149��$
��Bold"��� $
��Left=�$��47��$��	7,155,0,0q�$
 Wrap=�$��           Simulated Data��$	��Top=�$	��161���� -^   $	 inputDataButton��$��
NOT LOADED��$
��Left=�$��22��$��
13,176,0,0q�$	��Top=�$	��149��$
��Bold"�$ -0.517,-0.136&�� -_    �Name$ SimDataProgressBar�� � Value$ 0Q�
������$
��Left=�$��22��$��
13,176,0,0q�$	��Top=�$	��149��$
��Bold"�$ -0.517,-0.136&��� -`   $
 SimDataBorder��$��1q����	#FF727272$��1��$
��Left=�$��22��$��
13,176,0,0q�$	��Top=�$	��149��$ -0.517,-0.136&�._��� -a    ��Name$ SimDataTextBox��$��           NOT LOADED��$
��Bold"��� $
��Left=�$��90��$��	7,204,0,0q�$
 Wrap=�$��           Tool Geometry��$	��Top=�$	��161���� -b   $
 optionsPanel��
���   �$��1q�$
��Left=�$	��276��$��	4,469,0,0q�$	��Top=�$	��167��._��� $
��Left=�$��47��$��2,22,0,0q�$
 Wrap=�$��       Workpiece Geometry��$	��Top=�$	��161���� -c   $	 workPieceGeoButton��$��
NOT LOADED��$
��Left=�$��22��$��
13,513,0,0q�$	��Top=�$	��149��$
��Bold"�� -d   $ WorkPieceProgressBar��$ 0Q�
������$
��Left=�$��22��$��
13,513,0,0q�$	��Top=�$	��149��$
��Bold"�$ -0.517,-0.136&��� -e   $
 WorkPieceBorder��$��1q����	#FF727272$��1��$
��Left=�$��22��$��
13,513,0,0q�$	��Top=�$	��149��$ -0.517,-0.136&�._��� -f   $ WorkPieceTextBox��$��            NOT LOADED��$
��Bold"��� $
��Left=�$��20��$��	5,470,0,0q�$
 Wrap=�$��                OPTIONS��$	��Top=�$	��165��$
��Bold"����	#FF040983���	#FFFEFEFE�� $
��Left=�$��47��$��	7,542,0,0q�$
 Wrap=�$��           Data Recording��$	��Top=�$	��161���� -g   $	 outputDataFolderButton��$	��OFF��$
��Left=�$��22��$��
13,563,0,0q�$	��Top=�$	��149��$
��Bold"��� $
��Left=�$��47��$��	7,593,0,0q�$
 Wrap=�$!��           Kalman Filtering��$	��Top=�$	��161���� -h   $	 KalmanFilteringButton��$��NOT ACTIVATED��$
��Left=�$��22��$��
13,614,0,0q�$	��Top=�$	��149��$
��Bold"��� $
��Left=�$��47��$��	7,644,0,0q�$
 Wrap=�$��              TEST MODE��$	��Top=�$	��161���� -i   $	 TestOnButton��$	��OFF��$
��Left=�$��22��$��
13,665,0,0q�$	��Top=�$	��149��$
��Bold"�$ -0.517,-0.136&�� -j   $ TestOnProgressBar��$ 0Q�
������$
��Left=�$��22��$��
13,665,0,0q�$	��Top=�$	��149��$
��Bold"�$ -0.517,-0.136&��� -k   $
 TestOnBorder��$��1q����	#FF727272$��1��$
��Left=�$��22��$��
13,665,0,0q�$	��Top=�$	��149��$ -0.517,-0.136&�._��� -l   $ TestOnTextBox��$��                   OFF��$
��Bold"��� $
��Left=�$��47��$��	7,694,0,0q�$
 Wrap=�$��            Customization��$	��Top=�$	��161���� -m   $	 CustomizationButton��$��NOT ACTIVATED��$
��Left=�$��22��$��
13,715,0,0q�$	��Top=�$	��149��$
��Bold"�$ -0.517,-0.136&��� 
���   �$��1q����	#FFE5E5E5$
��Left=�$	��825��$��
172,34,0,0q�$	��Top=�$
��1225��._��� -n    ��Name$ tabControlMenu��$
��Left=�$	��825��$	��Top=�$
��1222��$��0,0,-1,0q�.@��� -o    ��Name$ TrackingTabItem��$��Tracking   ��$��-2,-2,-42,0q�.��� ���	#FFE5E5E5$��-2,-1,-5,-25q�.+��� -p   $��tracingListView��$
��Left=�$	��237��$	��Top=�$	��716��$��1,25,0,0q� �� View �� .P��� $��Line#�� �� Width$ 70�� �� DisplayMemberBinding ��  �� Path$ lineNumb��� $��G#��$ 40�� �� $ 	GFileNumb��� $��PosX(mm)��$ 75�� �� $
 posX��� $��PosY(mm)��$ 75�� �� $
 posY��� $��PosZ(mm)��$ 75�� �� $
 posZ��� $��Feed(mm/min)��$ 90�� �� $ feedRate��� $
��TFNC��$ 40�� �� $
 TFNC��� $��TFG(mm)��$ 60�� �� $	 TFG��� $��
Kt(N/mm^2)��$ 72�� �� $ coeffKt��� $��Force(N)��$ 90�� �� $ force��� $��SIMULATED INPUT DATA��$
��Left=�$��24��$	��Top=�$	��716��$
��Bold"�$��1,0,0,0q� � -q     �Name$ FollowerFrame��$��Tool Path Frame��$
��Left=�$	��472��$��
-1,297,0,0q�$	��Top=�$	��718�� � -r   $ FaRealTimeFrame��$��Fa Frame��$
��Left=�$	��199��$��	716,0,0,0q�$	��Top=�$	��507�� � -s   $ eta1RealTimeFrame��$��
eta1 Frame��$
��Left=�$	��198��$��716,200,0,0q�$	��Top=�$	��507�� � -t   $ FtORealTimeFrame��$��	FtO Frame��$
��Left=�$	��199��$��716,399,0,0q�$	��Top=�$	��507�� � -u   $ FeedrRealTimeFrame��$��FeedR Frame��$
��Left=�$	��199��$��716,599,0,0q�$	��Top=�$	��507���� -v   $
 RTcalculatedFrame��
���   �$��1q�$
��Left=�$��33��$��	1,263,0,0q�$	��Top=�$	��716��
����ح��� $��Kt [N/mm²]��$
��Left=�$��27��$��	6,263,0,0q�$	��Top=�$��89��$
��Bold"�$��14(��� -w    ��Name$ 	KtTextBox��$
��Left=�$��27��$��
98,266,0,0q�$
 Wrap=�$��0��$	��Top=�$��86��$��16(� �� TextAlignment$ Right=��� -x   $	 TabItemTestModeButton��$��TEST MODE ACTIVE��
��� � �$
��Left=�$��20��$��555,-23,0,0q�$	��Top=�$	��162��$
��Bold"�$y�Hidden=��� -y   $	 TabItemToolProcessButton��$��TOOL BREAKAGE��
����� �$
��Left=�$��20��$��718,-23,0,0q�$	��Top=�$	��122��$
��Bold"��� -z   $	 TabItemAdaptProcessButton��$��ADAPTIVE CONTROL��
����� �$
��Left=�$��20��$��845,-23,0,0q�$	��Top=�$	��122��$
��Bold"��� -{   $	 TabItemVirtualFeedButton��$��VIRTUAL/FEEDBACK��
����� �$
��Left=�$��20��$��972,-23,0,0q�$	��Top=�$	��122��$
��Bold"��� -|   $!	 TabItemChatterProcessButton��$��CHATTER CONTROL��
�������$
��Left=�$��20��$��1099,-23,0,0q�$	��Top=�$	��122��$
��Bold"��� -}   $
 RTcalculatedFrame_Copy��
���   �$��1q�$
��Left=�$��28��$��	1,770,0,0q�$	��Top=�$	��716��
��������� $,��&Unique Record File Name (Default: OCC)��$
��Left=�$��27��$��	0,770,0,0q�$	��Top=�$	��243��$
��Bold"��� -~   $ RecordNameTextBox��$
��Left=�$��22��$��247,773,0,0q�$
 Wrap=�$	��OCC��$	��Top=�$	��397��$
 Left=�$��14(��� -   $	 RecordNameButton��$��Default��$
��Bold"�$
��Left=�$��22��$��649,773,0,0q�$	��Top=�$��64���� -�   $ MonitoringTabItem��$��
Monitoring��$��43,-2,-81,0q�.��� .+�   WPF.MDI.MdiContainer  -�     Name$ MonitoringChildContainer���� -�   $
 SimX��$
��Left=�$��23��$��943,186,0,0q�$
 Wrap=�$��0��$	��Top=�$��61���� -�   $
 SimY��$
��Left=�$��23��$��1050,186,0,0q�$
 Wrap=�$��0��$	��Top=�$��63���� -�   $	 	Simulator��$��	Simulator��$
��Left=�$��26��$��943,227,0,0q�$	��Top=�$	��170���� -�   $
 RtmX��$
��Left=�$��23��$��943,294,0,0q�$
 Wrap=�$��0��$	��Top=�$��61���� -�   $
 RtmY��$
��Left=�$��23��$��1050,294,0,0q�$
 Wrap=�$��0��$	��Top=�$��63���� -�   $	 Realtime��$��Realtime��$
��Left=�$��26��$��943,335,0,0q�$	��Top=�$	��170���� $��Graphic Records��$��82,-2,-94,0q�.��� .+�  -�   $# DataRecordGraphChildContainer���� $��Data Records��$��95,-2,-123,0q�.��� .+�  -�   $" DataRecordTextChildContainer���� -�   $ CuttingTabItem��$��CNC Utilities��$��124,-2,-156,0q�.��� ���	#FFE5E5E5$��0,-1,0,0q�$
��1216��.+��� -�   $	 SetFeedRateButton��$��Set %��$
��Left=�$��25��$��1150,69,0,0q�$	��Top=�$��53���� -�   $ SetFeedRateTextBox��$
��Left=�$��25��$��1072,69,0,0q�$
 Wrap=�$��0��$	��Top=�$��62���� -�   $	 SetToolBreakage��$��Set ToolBreakage��$
��Left=�$��34��$��1072,264,0,0q�$	��Top=�$	��112���� $��Adjust FEED RATE��$
��Left=�$��26��$��1072,38,0,0q�$	��Top=�$	��131��$
��Bold"��� $��Adjust SPINDLE SPD��$
��Left=�$��26��$��1072,129,0,0q�$	��Top=�$	��131��$
��Bold"��� -�   $ SetSpindleSpeedTextBox��$
��Left=�$��25��$��1072,159,0,0q�$
 Wrap=�$��0��$	��Top=�$��62���� -�   $	 SetSpindleSpeedButton��$��Set %��$
��Left=�$��25��$��1150,159,0,0q�$	��Top=�$��53���� $��Adjust SPINDLE SPD��$
��Left=�$��26��$��1072,435,0,0q�$	��Top=�$	��131��$
��Bold"��� $��By VALUE��$
��Left=�$��26��$��1072,453,0,0q�$	��Top=�$	��131��$
��Bold"��� -�   $ SetSpindleSpdByValTextBox��$
��Left=�$��25��$��1072,483,0,0q�$
 Wrap=�$��0��$	��Top=�$��62���� -�   $	 SetSpindleSpdByValButton��$��Set Val��$
��Left=�$��25��$��1150,483,0,0q�$	��Top=�$��53���� -�   $
 optionsPanel_Copy��
���   �$��1q�$
��Left=�$	��168��$��	4,299,0,0q�$	��Top=�$	��167��._�� -�    �Name$ CNCrealtimePanel�����	#FFE5E5E5.+��� $
��Left=�$��20��$
 Wrap=�$��       Real-Time CNC Data��$	��Top=�$	��165��$
��Bold"����	#FF126648���	#FFFEFEFE�� -�   $ RealTimeXtextBox��$
��Left=�$��23��$��	65,25,0,0q�$
 Wrap=�$��0.0000��$	��Top=�$��97��$
��Bold"�$��18(�$ Right=��� -�   $ RealTimeYtextBox��$
��Left=�$��23��$��	65,53,0,0q�$
 Wrap=�$��0.0000��$	��Top=�$��97��$
��Bold"�$��18(�$ Right=��� -�   $ RealTimeZtextBox��$
��Left=�$��23��$��	65,81,0,0q�$
 Wrap=�$��0.0000��$	��Top=�$��97��$
��Bold"�$��18(�$ Right=�$ 0.421,3.043&��� -�   $ RealTimeFeedRateTextBox��$
��Left=�$��23��$��
65,109,0,0q�$
 Wrap=�$��0��$	��Top=�$��97��$
��Bold"�$��18(�$ Right=��� -�   $ RealTimeSpindleTextBox��$
��Left=�$��23��$��
65,137,0,0q�$
 Wrap=�$��0��$	��Top=�$��97��$
��Bold"�$��18(�$ Right=��� $��X (mm)��$
��Left=�$��33��$��	19,25,0,0q�$	��Top=�$��61���� $��Y (mm)��$
��Left=�$��33��$��	19,52,0,0q�$	��Top=�$��61���� $��Z (mm)��$
��Left=�$��33��$��	19,80,0,0q�$	��Top=�$��61���� $��	Feed Rate��$
��Left=�$��33��$��	3,110,0,0q�$	��Top=�$��67���� $��Spindle��$
��Left=�$��33��$��16,138,0,-5q�$	��Top=�$��52���� $��Diameter (mm)��$
��Left=�$��32��$��	5,222,0,0q�$	��Top=�$��90���� $��Lenght (mm)��$
��Left=�$��26��$��
17,244,0,0q�$	��Top=�$��89���� -�   $ ToolDiameterTextBox��$
��Left=�$��20��$��
93,224,0,0q�$
 Wrap=�$��0.0000��$	��Top=�$��72��$ Right=�$
��Bold"����	#FFD8D8D8$��14(��� -�   $ ToolLenghtTextBox��$
��Left=�$��20��$��
93,247,0,0q�$
 Wrap=�$��0.0000��$	��Top=�$��72��$ Right=�$
��Bold"����	#FFD8D8D8$��14(��� -�   $ ToolNumbOfTeethTextBox��$
��Left=�$��20��$��
93,271,0,0q�$
 Wrap=�$��0��$	��Top=�$��72��$ Right=�$
��Bold"����	#FFD8D8D8$��14(��� $��       # of Teeth��$
��Left=�$��32��$��	5,268,0,0q�$	��Top=�$��90���� -�   $��ImageMAL��$��96��$��
38,751,0,0q�$	��Top=�$��94��$��UBC.ico��$
��Left=��� 
���   �$��1q�$
��Left=�$	��113��$��	4,746,0,0q�$	��Top=�$	��166��._��� $*��$ Manufacturing Automation Laboratory��$��Right=�$��28��$��
0,92,-1,-9q�$	��Top=�$	��165��$��8(�$
��Bold"�