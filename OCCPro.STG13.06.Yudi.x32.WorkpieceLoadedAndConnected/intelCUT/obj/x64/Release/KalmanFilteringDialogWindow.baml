   M S B A M L   `   `   `  ����   intelCUT=    7OnlineCuttingControlProcess.KalmanFilteringDialogWindow   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35U QWPFToolkit, Version=3.5.40128.1, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml       � Title$(  "Kalman Filtering Data Entry Dialog��$	��456��$	��742�� � WindowStartupLocation$ CenterScreen=� � 
ResizeMode$ NoResize=� � Icon$ CNCMach.ico��.��� 
�������.+��� -    ��Name$ KalmanWindowButton��$��X��
�����F�
�������$
��Left=�$��32��$��2,1,0,0q�$	��Top=�$��43��$
��Bold"�$��24(��� 
���   �
����ح�$��1q�$
��Left=�$��32��$��46,1,0,0q�$	��Top=�$	��678��._�� .+��� $��Matrix Data:��$
��Left=�$��28��$��2,0,0,0q�$	��Top=�$	��112��$
��Bold"�$��14(��� -    ��Name$ StatusLabel��$��Empty��$��27��$��	93,2,10,0q�$	��Top=��� 
���   �
�������$��1q�$
��Left=�$	��381��$��2,34,0,0q�$	��Top=�$	��137��._�� .+��� $��Data Entry Method��$
��Left=�$��25��$��9,3,0,0q�$	��Top=�$	��117��$
��Bold"��� -    ��Name$ DataEntryComboBox��$
��Left=�$��25��$��	13,30,0,0q�$	��Top=�$	��110�� �� SelectedIndex$ 0���� 
�������
�������$��1q�$
��Left=�$��98��$��2,59,0,0q�$	��Top=�$	��131��._�� .+��� $��Matrix Indexes��$��25��$��	14,-2,0,0q�$	��Top=�$��95��$
��Bold"�$
��Left=��� $��State#��$
��Left=�$��25��$	��Top=�$��43��$��	44,21,0,0q��� -   $ StateComboBox��$
��Left=�$��19��$��	85,24,0,0q�$	��Top=�$��42��$ 0��{�.  �� $��Measurement#��$
��Left=�$��25��$	��Top=�$��91��$��	-3,44,0,0q��� -   $ MeasureComboBox��$
��Left=�$��19��$��	85,46,0,0q�$	��Top=�$��42��$ 0��{�.  �� -   $ EnterIndexButton��$��Enter��$
��Left=�$��	63,75,0,0q�$	��Top=�$��59��$��19��{�.  �� 
�������
�������$��1q�$
��Left=�$	��117��$��	2,158,0,0q�$	��Top=�$	��131��._�� .+��� $��Matrix Datafill��$��25��$��	14,-4,0,0q�$	��Top=�$��95��$
��Bold"�$
��Left=��� $��Matrix��$
��Left=�$��25��$	��Top=�$��48��$��	-3,18,0,0q�$
��Bold"��� -   $ MatrixTypeComboBox��$
��Left=�$��19��$��	43,22,0,0q�$	��Top=�$��84��$ 0��{�.  $��10(��� $��Fill Up��$
��Left=�$��25��$	��Top=�$��46��$��	-3,40,0,0q�$
��Bold"��� -   $ FillUpTypeComboBox��$
��Left=�$��19��$��	43,44,0,0q�$	��Top=�$��84��$ 0��{�.  $��10(��� $
��With��$
��Left=�$��25��$	��Top=�$��36��$��7,62,0,0q��� -	    ��Name$ FillUpDataTextBox��$
��Left=�$��19��$��	43,66,0,0q�	 �� TextWrapping$
	 Wrap=�$	��Top=�$��84��{�.  �� -
   $ EnterFillButton��$��Enter��$
��Left=�$��	63,93,0,0q�$	��Top=�$��59��$��19��{�.  �� -   $ 
SaveButton��$
��Save��$
��Left=�$��22��$��
55,288,0,0q�$	��Top=�$��75���� -   $ ClearButton��$��Clear��$
��Left=�$��22��$��
55,314,0,0q�$	��Top=�$��75���� -   $ 
OkayButton��$��OK��$
��Left=�$��24��$��
71,349,0,0q�$	��Top=�$��57��$
��Bold"��� -   
 ��Name$
 	MatrixTab��$
��Left=�$	��381��$��
141,34,0,0q�$	��Top=�$	��583��.@��� -    ��Name$ 	A_TabItem��$��   A   ��.��� ���	#FFE5E5E5.+�&   System.Windows.Controls.DataGrid  -     Name$ A_MatrixGrid��$��Stretch=�   GridLinesVisibility$	 All=�   CanUserAddRows .     CanUserDeleteRows .     CanUserReorderColumns .     CanUserSortColumns .     HeadersVisibility$	 All=�   SelectionMode$ Extended=�   SelectionUnit$ CellOrRowHeader=�   AlternatingRowBackground
 ����� �� AlternationCount$ 2��   AutoGenerateColumns . ���� ����  �� 
TargetType)  #System.Windows.Controls.DataGridRow"  .���  �� Property � y� �� Value ��    RowHeaderTemplate �� .R��� ����  �� RelativeSource ��  �� Mode$ FindAncestor=� �� AncestorType"   �� Path$ Header��� -   $ 	P_TabItem��$��   P   ��.��� ���	#FFE5E5E5.+�  -   $ P_MatrixGrid��$��Stretch=�$	 All=� .   .   .   .  $	 All=�$ Extended=�$ CellOrRowHeader=�
 �����$ 2�� . ���� ���� "  .���  � y� ��  �� .R��� ����  �� $ FindAncestor=�"  $ Header��� -   $ 	Q_TabItem��$��   Q   ��.��� ���	#FFE5E5E5.+�  -   $ Q_MatrixGrid��$��Stretch=�$	 All=� .   .   .   .  $	 All=�$ Extended=�$ CellOrRowHeader=�
 �����$ 2�� . ���� ���� "  .���  � y� ��  �� .R��� ����  �� $ FindAncestor=�"  $ Header��� -   $ 	H_TabItem��$��   H   ��.��� ���	#FFE5E5E5.+�  -   $ H_MatrixGrid��$��Stretch=�$	 All=� .   .   .   .  $	 All=�$ Extended=�$ CellOrRowHeader=�
 �����$ 2�� . ���� ���� "  .���  � y� ��  �� .R��� ����  �� $ FindAncestor=�"  $ Header��� -   $ 	R_TabItem��$��   R   ��.��� ���	#FFE5E5E5.+�  -   $ R_MatrixGrid��$��Stretch=�$	 All=� .   .   .   .  $	 All=�$ Extended=�$ CellOrRowHeader=�
 �����$ 2�� . ���� ���� "  .���  � y� ��  �� .R��� ����  �� $ FindAncestor=�"  $ Header��� -   $ 	K_TabItem��$��   K   ��.��� ���	#FFE5E5E5.+�  -   $ K_MatrixGrid��$��Stretch=�$	 All=� .   .   .   .  $	 All=�$ Extended=�$ CellOrRowHeader=�
 �����$ 2�� . ���� ���� "  .���  � y� ��  �� .R��� ����  �� $ FindAncestor=�"  $ Header�