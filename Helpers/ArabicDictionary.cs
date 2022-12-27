using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NowBuySell.Web.Helpers
{
    public static class ArabicDictionary
    {
        static Dictionary<string, string> Arabic = new Dictionary<string, string>
        {
            {"Automatic","تلقائي"},
            {"Manual","يدوي"},
            {"CVT","CVT"},
            {"DCT","DCT"},
            {"Doors","أبواب"},
            {"Wheels","عجلات"},
            {"CC","سم مكعب"},
            {"Beds","سرير"},
            {"Baths","الحمامات"},
            {"Dinings","السفرة"},
            {"Laundry","غسيل ملابس"},
            {"Sale","تخفيض السعر"},
            {"Rent","تأجير"},
            {"Garage","كراج"},
            {"Sqft","قدم مربع"},
            {"Petrol","بنزين"},
            {"Diesel","ديزل"},
            {"Hybrid","هجين"},
            {"Electric","كهربائي"},
            {"Persons","الأشخاص"},
            {"Left-Hand Side","الجانب الأيسر"},
            {"Right-Hand Side","الجانب الأيمن"},
            {"Perfect inside","مثالي من الداخل"},
            {"Perfect out","خارج الكمال"},
            {"Perfect inside & out","مثالي من الداخل والخارج"},
            {"N/A, Electric","غير متاح ، كهربائي"},
            {"Not Sure","غير متأكد"},
            {"HP","حصان"},
            {"Less Than 150 HP","أقل من 150 حصان"},
            {"150 - 200 HP","حصان 150 - 200"},
            {"200 - 300 HP","200 - 300 حصان"},
            {"GCC Specs","المواصفات الخليجية"},
            {"European Specs","المواصفات الأوروبية"},
            {"Japanese Specs","المواصفات اليابانية"},
            {"American Specs","المواصفات الأمريكية"},
            {"Canadian","كندي"},
            {"Australian Specs","المواصفات الاسترالية"},
            {"Other Specs","المواصفات الأخرى"},
            {"Thank you! Your message has been successfully sent.","شكرا لك! تم إرسال رسالتك بنجاح."},
            {"Oops! Something went wrong. Please try later.","وجه الفتاة! هناك خطأ ما. من فضلك حاول لاحقا."},
            {"Bad request!","اقتراح غير جيد!"},
            {"Meeting Request Sent successfully","تم إرسال طلب الاجتماع بنجاح"},
            {"Notification added successfully","تمت إضافة الإخطار بنجاح"},
            {"Authorization failed for current request","فشل التفويض للطلب الحالي"},
            {"Account created successfully","الحساب اقيم بنجاح"},
            {"Customer added successfully","تمت إضافة العميل بنجاح"},
            {"Customer already exist","الزبون موجود بالفعل"},
            {"User already exists","المستخدم موجود اصلا"},
            {"Cool! Password recovery instruction has been sent to your email.","بارد! تم إرسال تعليمات استعادة كلمة المرور إلى بريدك الإلكتروني."},
            {"Invalid Email","بريد إلكتروني خاطئ"},
            {"Email required","البريد الإلكتروني (مطلوب"},
            {"Please upload file first","الرجاء تحميل الملف أولا"},
            {"Cool! OTP Verified","بارد! تم التحقق من OTP"},
            {"OTP Code Incorrect","رمز OTP غير صحيح"},
            {"OTP Code Expired","انتهت صلاحية رمز OTP"},
            {"Invalid Contact Number","رقم الاتصال غير صحيح"},
            {"Phone Number required","رقم الهاتف مطلوب"},
            {"OTP Sent","تم إرسال OTP"},
            {"Contact Number required","رقم الاتصال مطلوب"},
            {"Incorrect password","كلمة سر خاطئة"},
            {"Account suspended!","حساب معلق!"},
            {"Vendor updated successfully","تم تحديث البائع بنجاح"},
            {"Vendor already exist","البائع موجود بالفعل"},
            {"Password Changed Successfully","تم تغيير الرقم السري بنجاح"},
            {"The current password is wrong!","كلمة المرور الحالية خاطئة!"},
            {"Session invalid or expired","الجلسة غير صالحة أو منتهية الصلاحية"},
            {"Logged out successfully","تم تسجيل الخروج بنجاح"},
            {"Subscriber added successfully","تمت إضافة المشترك بنجاح"},
            {"Subscriber already exist","المشترك موجود بالفعل"},
            {"Request Sent successfully","تم إرسال الطلب بنجاح"},
            {"Please fill the form properly.","يرجى ملء النموذج بشكل صحيح."},
            {"Product added to wishlist","تمت إضافة المنتج إلى قائمة الرغبات"},
            {"Wish already exist in the list","الرغبة موجودة بالفعل في القائمة"},
            {"Wish deleted from list successfully","تم حذف الرغبات من القائمة بنجاح"},
            {"Kindly add this product in wishlist first","يرجى إضافة هذا المنتج في قائمة الرغبات أولا"},
            {"Contact number is being used by another account","يتم استخدام رقم الاتصال من قبل حساب آخر"},
            {"Profile updated!","تحديث الملف الشخصي!"},
            {"Customer updated successfully","تم تحديث العميل بنجاح"},
            {"Email is already registered to other customer","البريد الإلكتروني مسجل بالفعل لعميل آخر"},
            {"Account already exist!","الحساب موجود مسبقا!"},
            {"Contact already in use","جهة الاتصال قيد الاستخدام بالفعل"},
            {"Verify your contact first","تحقق من الاتصال الخاص بك أولا"},
            {"OTP sent successfully","تم إرسال OTP بنجاح"},
            {"Contact not found","جهة الاتصال غير موجودة"},
            {"Your OTP has been expired","لقد انتهت صلاحية OTP الخاص بك"},
            {"Contact verified","تم التحقق من الاتصال"},
            {"Invalid OTP","OTP غير صحيح"},
            {"OTP resend successfully","إعادة إرسال OTP بنجاح"},
        };

        public static string Translate(string search, bool AllowSplit = true)
        {
            try
            {
                if (AllowSplit)
                {
                    string value = string.Empty;
                    var split = search.Split(' ');
                    string ar = Arabic[split.LastOrDefault()];

                    for (int i = 0; i < split.Count() - 1; i++)
                        value += split[i] + " ";

                    return value + ar;
                }
                else
                {
                    return Arabic[search];
                }
            }
            catch(Exception ex)
            {
                return search;
            }
        }
    }
}