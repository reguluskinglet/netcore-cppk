   M S B A M L   `   `   `  ���� G  CTabletLocker, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null+clr-namespace:TabletLockerTabletLocker  7 clr-namespace:TabletLocker.UtilsTabletLocker.Utils  !    TabletLocker.AdminLoginPage   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     ,localclr-namespace:TabletLocker     X TXceed.Wpf.Toolkit, Version=3.4.0.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f44xctk)http://schemas.xceed.com/wpf/xaml/toolkit  ,utils clr-namespace:TabletLocker.Utils    �� IsFlicksEnabled  .   N� Title$ AdminLoginPage��.��� .+��� -    ��Name$ AdminCanvas��
������� J� Effect -  'System.Windows.Media.Effects.BlurEffect  -   AdminCanvasBlur��   Radius$ 0Q�.+��� .+�F� ���	#ffffffff4����     �/� @KL �/� @KL    �        �     �/�� .+�F� 4����        � @KL    � @KL ��     ��        $��151.241�� �� Stretch$
 Fill=�����  � MappingMode$ Absolute=� �� 
StartPoint$ -1623.667,-192.250&� �� EndPoint$ -343.667,-192.250&���� 	 � Offset$
	 0.00Q�
 � Color$
 	#ff009cc6��� $
	 0.82Q�$
 	#ff0080b9�� �� 	Transform u�  u� Matrix$. (1.000,0.000,-0.000,-1.000,1632.000,0.000x��� .+�F� ���	#ff0084c24���� @KL Zb�     Zb�        � @KL    � @KL Zb�� -    ��Name$ TimeMark�����	#ffffffff �� Left$ 38��
 �� Top$ 9��$��21.58(�$��Roboto Medium)�12:30�� -   $ DateMark�����	#ffffffff$
 1130��$ 9��$��21.58(�$��Roboto Medium)�
05.04.2018�� .+�F�  J� Opacity$	 0.3Q����	#ffffffff9����    ��@ Zb�     Zb�     V	� @KL V	� @KL Zb�� -    ��Name$ BtnBack��$
 1200��$ 75������  �� 
TargetType" ��.R��� 
������ ._�� .+�F� $��10,0,0,0q����	#ffffffffa�����	LI��1�@I�����%[H ظ�@Ix�t��	LI�?�p�Hm�ј�VJm�ј�VJ�B��p�H�B���	LI��1$��24.668��$
 Fill=�$��33.416��F� $	��3.0�����	#ffffffff �� StrokeMiterLimit$	 1.0Q�����Ј�KJ��7ш�KJ�l�ӈ�KJ x���J!�h��I!���;H!����G!��^�G x��^�G�l�Ѹ^�Gp��Ӹ^�G@����GP�u�;HP�u�h��IP�uӀ�JP�u��KJ@����KJp��ш�KJ��C$��42��$
 Fill=�$��42.501���� .+�F� $	 0.5Q����	#ffffffff�������H��Ӱ{x�uP�I`�j��z`�j�P�`�j�/������H�Y�X���qx3��(� /�H>��pu��t��@������FX;��zX;Ӹy)X;q=
ף8P@��T01�������� L� ������H��F� $	 0.5Q����	#ffffffff����� ��Y�Ѩ��]�ј
g�]�� �*����&��I��z�I�P���IX�.8InX�.���X�.�tp�B��5(�d�v��U�(��F`H��v6����Ȃ4P��Ȏ2�����0(������2h����rh���"��h��P���X��3��z�3�Ȩ��3�Vh��x��p���q�������(� ��Y�F� $	 0.5Q����	#ffffffffH����h�+�c�Ӡ�P#�`Y�I�@��`p��`$�`p�ѐ��c��h�+�c�F� $	 0.5Q����	#ffffffff�����Ȁ��%���x�(��0n�F%�P|x����ax���P�,x�������xk����xe����Ӑ�����x���[� ���[��Zd;߷P@�[�(;��P"��J���Ѱ�J�ӨWȥ�H�n���Bw�D��3y8��x�| ����}����8~� ���k�%��T�%�P �%�S�%����&0���'Ӏ���&S�%P �%�P���%јv��Cm�Pin�Cm��j�Cm�j�CmPin�Cmр��Cm� ��h�i���(Z;���`}����P� �����Ȁ��%�� $ 76��$
��1280��.+��� ���	#ffffffff$��Center=�$��36.00(�$��Calibri Light)�53Авторизация администратора� $	 207��$��Center=�$
��1280��.+��� $��Center=�.+�� $��Center=�$	��170��$	��140��   RzdLogo#��[  (  "Xceed.Wpf.Toolkit.WatermarkTextBox  -     Name$ 
LoginInput��,   &TabletLocker.Utils.TouchScreenKeyboard   TouchScreenKeyboard . $��0,33,0,0q�$��Center=�$��Center=�   	Watermark$ 
Логин�����	#ff004c66$��39.00(�$��Calibri Light)�$	��592��$��80��
����������	#ff9eaab7$��1q�,  &Xceed.Wpf.Toolkit.WatermarkPasswordBox  -     Name$ PasswordInput�� .    PasswordChar$ *��$��0,33,0,0q�$��Center=�$��Center=�$ Пароль�����	#ff004c66$��39.00(�$��Calibri Light)�$	��592��$��80��
����������	#ff9eaab7$��1q�.��
password�� -   $ BtnLogin��$��0,33,0,0q�$��
Войти��$��96��$	��592��  ButtonLoginStyle#���  �� -	   $ MsgBox��.+�F� $	 0.2Q����	#ff111213M�����I���@0�xT��I�	�@0�xT��I�	�@0�-��I���@0�-��I���@0�xT$	��800��$
 Fill=�$
��1280��F� ���	#ffffffffM�����I��@0�J9��I���@0�J9��I���@0�@��I��@0�@��I��@0�J9$	 388��$	 148��$	��504��$
 Fill=�$	��504���� -
   $ MsgBoxTitle�� �� TextAlignment$ Center=����	#ff004c66$	 389��$	 382��$��39.00(�$��Calibri Light)�$	��501��%#Ошибка авторизации�� -   $ MsgBoxMessage��$ Center=����	#ff004c66$	 389��$	 439��$��31.00(�$��Calibri Light)�$	��501��53Не удалось считать штрихкод�� -   $
��ОК��$��87��$	��430��$	 426��$	 519��  ButtonMsgboxOkStyle#���  �� .+�F� $	��3.0�����	#ff194180$	 1.0Q������V-y�@�k�"�V-y�@P��$V-�]<�@ج�&1�Z�@ج�&�+�V��@ج�&��阮i�@P��$��阮i�@�k�"���阮i�@�R +�V��@p&�1�Z�@p&��V-�]<�@p&�V-y�@�R V-y�@�k�"$	 565��$	 198��$	��151��$
 Fill=�$	��148��F� ���	#ff194180�����������@9�#��v���@Zd;߄�@�G�z���@Zd;߄�@���S����@9�#�������@9�#���K7��@���$���K7��@�tk$������@��3$����Y�@��3$�)\��u��@��3$�I��@�tk$�I��@���$��I��@0�%)\��u��@�b=%����Y�@�b=%�/�$��@�b=%��K7��@0�%��K7��@���$$	 629��$	 218��$	��107��$
 Fill=�$��17��