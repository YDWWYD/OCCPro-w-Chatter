   M S B A M L   `   `   `  ����  intelCUT8    2OnlineCuttingControlProcess.ConnectionDialogWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35U QWPFToolkit, Version=3.5.40128.1, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$  Connection Dialog��5   	   $	��326��6#   $	��514��60    � Left$	 500��6<   
 � Top$	 300��6G    � Icon$ connect.ico��6Q   .��5      � $��0,0,2,-1q�6   .+�5   
   �� -   5         ��Name$ textName��5      $
��Left=�6$   $��21��6?   $��
368,56,0,0q�6K    �� TextWrapping$
 Wrap=�6_   $	��Top=�6s   $	��120��6�   �� 5   
   -   5        $ textIP��5      $
��Left=�6"   $��21��6=   $��
368,82,0,0q�6I   $
 Wrap=�6]   $	��Top=�6q   $	��120��6�   6�   �� 5   
   -   5         ��Name$ 	addButton��5      $	��Add��6$   $
��Left=�62   $��23��6M   $��408,120,0,0q�6Y   $	��Top=�6n   $��80��6�   6�   �� 5	   
   -   5         ��Name$ cncNameList��5	      $
��Left=�6'   $	��180��6B   $��	11,56,0,0q�6O   $	��Top=�6b   $	��120��6z   6�   �� 5
   
   -   5        $ cncIPAddress��5
      $
��Left=�6(   $	��181��6C   $��
134,56,0,0q�6P   $	��Top=�6d   $	��120��6|   {�.   J� RenderTransformOrigin$ 0.383,0.486&�6�   �� 5   
   $
��Left=�6   $��15��6/   $��	11,38,0,0q�6;   $
��Wrap=�6N   $��CNC Machine Name��6b   $	��Top=�6z   $	��144��6�   �� 5   
   $
��Left=�6   $��19��6/   $��
136,38,0,0q�6;   $
��Wrap=�6O   $��
IP Address��6c   $	��Top=�6u   $	��120��6�   �� 5   
   -   5        $ deleteButton��5      $��Delete��6'   $
��Left=�68   $��23��6S   $��270,215,0,0q�6_   $	��Top=�6t   $��80��6�   6�   �� 5   
   $
��Left=�6   $��20��6/   $��
293,57,0,0q�6;   $
��Wrap=�6O   $��
Name Entry��6c   $	��Top=�6u   $��70��6�   �� 5   
   $
��Left=�6   $��20��6/   $��
294,82,0,0q�6;   $
��Wrap=�6O   $��IP Entry��6c   $	��Top=�6s   $��69��6�   �� 5   
   -   5        $ testPingButton��5      $��	Ping Test��6)   $
��Left=�6=   $��23��6X   $��
11,256,0,0q�6d   $	��Top=�6x   $	��120��6�   {�.  6�   �� 5   
   -   5        $ pingingOkay��5      $��Connect��6&   $
��Left=�68   $��23��6S   $��408,256,0,0q�6_   $	��Top=�6t   $��80��6�   {�.  $��14(�6�   $
��Bold"�6�   �� 5   
   $
��Left=�6   $��23��6/   $��	11,10,0,0q�6;   $
��Wrap=�6N   $��Status:��6b   $	��Top=�6q   $��46��6�   $
��Bold"�6�   �� 5   
   -	   5        	 ��Name$	 pingingStatusDisplay��5      $
��Left=�62   $��23��6M   $��	61,10,0,0q�6Y   $
��Wrap=�6l   $��No Connection��6�   $	��Top=�6�   $	��427��6�   5      5      