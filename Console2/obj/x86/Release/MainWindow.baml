   M S B A M L   `   `   `  ����   Console2    Console2.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml       � Title$%  Universal Balise/LEU Programmer��$	��827��$	��595��.���� $
��Auto=�$	��776��.��� $	��776��.+��� -   $��	groupBox1��$��Manufacturer��$	��100��$��Center=�$��12,0,0,0q�$	��Top=�$	��529��.��� $��Stretch=�$��Stretch=� � ColumnDefinitions �� $��* ��� $��* ��� $��* ��� $��* �.+�	� -   $��manu_shinwoo��$��ShinWoo��$��0��$��Center=�$��Center=� d� 	IsChecked$
 TrueY�	� -   $��manu_bombardier��$��
Bombardier��$��1��$��Center=�$��Center=�	� -   $��manu_siemens��$��Siemens��$��2��$��Center=�$��Center=�	� -   $��manu_others��$��Others��$��3��$��Center=�$��Center=��� -   $��position_gpbox��$��Position Information��$	��187��$��Center=�$��21,178,257,0q�$	��Top=�$	��279��.��� .+��� -   $��label1��$��Country code: ��$��28��$
��Left=�$��	26,20,0,0q�$	��Top=��� -   $��label2��$��Group code:��$��28��$
��Left=�$��	26,46,0,0q�$	��Top=��� -	   $��label3��$��Position code:��$��28��$
��Left=�$��	26,72,0,0q�$	��Top=��� -
   $��countrycode_txt��$��20��$	��887�� �� TextAlignment$ Center=�$
��Left=�$��117,24,0,00q�$	��Top=�$	��114���� -   $��groupcode_txt��$��20��$ Center=�$
��Left=�$��
117,50,0,0q�$	��Top=�$	��114���� -   $��poscode_txt��$��20��$ Center=�$
��Left=�$��
117,76,0,0q�$	��Top=�$	��114��� $��Stretch=� �� $��* ��� $��* ��� $��* �.+��� -   $��posread_btn��$
��Read��$��23��$��Center=�$��0,0,0,20q�$��Bottom=�$��75��$��0���� -   $��poswrite_btn��$��Write��$��23��$��Center=�$��0,0,0,20q�$��Bottom=�$��75��$��1���� -   $��	reset_btn��$��Reset��$��23��$��Center=�$��0,0,0,20q�$��Bottom=�$��75��$��2���� -   $��telegram_gpbox��$��Telegram��$	��187��$��Center=�$��303,178,0,0q�$	��Top=�$	��239��.��� .+��� -   $��device_gpbox��$��Device��$��82��$
��Left=�$��6,16,0,0q�$	��Top=�$	��216��$��3��.���  �� $��* ��� $��* ��� $��* �.+�	� -   $��
pos_balise��$��Balise��$��16��$��Center=�$��Center=�$��0��$
 TrueY�	� -   $��pos_leu��$	��LEU��$��16��$��Center=�$��Center=�$��1��	� -   $��
pos_others��$��Others��$��16��$��Center=�$��Center=�$��2���� -   $��download_btn��$��Download��$��23��$��Center=�$��0,0,0,20q�$��Bottom=�$��65��$��0���� -   $��
upload_btn��$��Upload��$��23��$��Center=�$��0,0,0,20q�$��Bottom=�$��65��$��1���� -   $��
verify_btn��$��Verify��$��23��$��Center=�$��0,0,0,20q�$��Bottom=�$��65��$��2�� �� $��* ��� $��* ��� $��* ��� -   $��msgbox��$	��266��$��Center=�$��
13,474,0,0q�$	��Top=�$	��529��" �� VerticalScrollBarVisibility$ Visible=� � 	Resources  �� 
TargetType �� Property �� Value��%3   'J�       �� " J�.���  �@��Margin$ 0q��� -   $��msgClear_btn��$��Clear��$��23��$��Center=�$��467,746,0,0q�$	��Top=�$��75���� -   $��msgSave_btn��$
��Save��$��23��$��Center=�$��389,746,91,0q�$	��Top=�$��75���� -   $��msgDecode_btn��$��Decode��$��23��$��Center=�$	��Top=�$��75��$��20,746,460,0q��� -   $��serial_gpbox��$��Serial Port��$��66��$��Center=�$��
12,371,0,0q�$	��Top=�$	��529��.��� .+��� -   $��label4��$��Serial Port: ��$��28��$
��Left=�$��11,6,0,0q�$	��Top=��� -   $��serialportbox��$��23��$
��Left=�$��	100,9,0,0q�$	��Top=�$	��177��	 �� 
IsEditable	 . �� -   $��serialConnect_btn��$��Connect��$��23��$
��Left=�$��	432,7,0,0q�$	��Top=�$��75��� -    $��progressBar��$��25��$��Center=�$	��Top=�$	��528��$��
13,443,0,0q��� -!   $��progressLabel��$��Center=�$��0,447,0,509q�$	��200��
 �� TextAlignment$
 Center=��� -"   $��balisetest_gpbox��$��Balise Test��$��65��$��Center=�$��
12,106,0,0q�$	��Top=�$	��529��.���  �� $��* ��� $��* ��� $��* ��� $��* �.+��� -#   $��telRead_btn��$��Telegram Read��$��0��$��23��$��Center=�$��Center=�$��98���� -$   $��tempinout_btn��$��간이 입출력 특성��$��1��$��23��$��Center=�$��Center=�$��98���� -%   $��	inout_btn��$��정밀 입출력 특성��$��2��$��23��$��Center=�$��Center=�$��98���� -&   $��telStop_btn��$
��Stop��$��3��$��23��$��Center=�$��Center=�$��98��