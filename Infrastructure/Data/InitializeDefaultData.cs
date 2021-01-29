using Core.Models.Blogs;
using Core.Models.Error;
using Core.Models.Identity;
using Core.Models.Settings;
using Core.Models.Uploads;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data
{
    public static class InitializeDefaultData
    {

        /************************************************************************************
        in the API/program.cs execute this class to add Roles, SuperAdmin and Seed users data
        *************************************************************************************/

        // all default Roles and Users
        public static string[] ROLES = {
             "SuperAdmin", "Admin", "Editor", "Visitor"
         };
        private static string superAdmin_id;
        static CultureInfo MyCultureInfo = new CultureInfo("de-DE");
        public static async Task<bool> AddDefaultRolesInDatabase(IServiceProvider services)
        {
            // Get RoleManager Service to Application
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            if (!roleManager.Roles.Any())
            {
                for (int i = 0; i < ROLES.Length; i++)
                {
                    // create the Role
                    IdentityRole role = new IdentityRole();
                    if (!roleManager.RoleExistsAsync(ROLES[i]).Result)
                    {
                        role.Name = ROLES[i];
                        await roleManager.CreateAsync(role);
                    }
                }
                return true;
            }
            else
                return false;
        }

        public static async Task AddDefaultUplodTypes(IServiceProvider services)
        {
            var context = services.GetRequiredService<AppDbContext>();

            // for now just the image types, maybe leater add fileCV, fileCoverletter, ...
            string[] types = { "imageProfile", "imageCover", "imageBlog" };

            for (int i = 0; i < types.Length; i++)
            {
                var type = new UploadType
                {
                    Name = types[i]
                };
                await context.UploadType.AddAsync(type);
            }
            await context.SaveChangesAsync();
        }

        public static async Task AddSuperAdminUser(IServiceProvider services)
        {
            // Get UserManager Service to Application
            UserManager<AppUser> userManager = services.GetRequiredService<UserManager<AppUser>>();

            if (!userManager.Users.Any())
            {
                var user = new AppUser
                {
                    Email = "ahmad@al-alloush.com",
                    EmailConfirmed = true,
                    UserName = "Al-Alloush",
                    FirstName = "Ahmad",
                    LastName = "Alloush",
                    SelectedLanguages = "ar,en,",
                    RegisterDate = DateTime.Now,
                    Birthday = DateTime.Parse("15/6/1978", MyCultureInfo),
                    PhoneNumber = "1734522",
                    PhoneNumberConfirmed = true,
                    AutomatedPassword = false,
                    Address = new Address
                    {
                        Street = "Bahn",
                        BuildingNum = "12",
                        Flore = "L 3",
                        City = "Diennsried",
                        Zipcode = "874563",
                        Country = "German"
                    }
                };

                string Password = "!QA1qa";
                IdentityResult create = await userManager.CreateAsync(user, Password);
                if (create.Succeeded)
                {
                    superAdmin_id = user.Id;
                    userManager.AddToRoleAsync(user, ROLES[0]).Wait();
                }

                var context = services.GetRequiredService<AppDbContext>();

                // add default image for SuperAdmin in upload and UserImage tables
                string[] imgName = { "superAdmin", "superAdminCover", "user (10)", "user (15)" };
                for (int i = 0; i < imgName.Length; i++)
                {
                    var typ = i + 1;
                    var def = true;
                    if (i > 1)
                    {
                        // just the first two images are default
                        typ = 3;
                        def = false;
                    }

                    var upload = new Upload
                    {
                        Name = imgName[i],
                        Path = "/Uploads/Images/" + imgName[i] + ".jpg",
                        AddedDateTime = DateTime.Now,
                        UserId = superAdmin_id
                    };
                    await context.Upload.AddAsync(upload);
                    await context.SaveChangesAsync();

                    var userImage = new UploadUserImagesList
                    {
                        UploadId = upload.Id,
                        UserId = superAdmin_id,
                        Default = def,
                        UploadTypeId = typ
                    };
                    await context.UploadUserImagesList.AddAsync(userImage);
                    await context.SaveChangesAsync();
                }
            }
        }

        public static async Task AddDefaultErrorTypes(IServiceProvider services)
        {
            var context = services.GetRequiredService<AppDbContext>();

            string[] errorTypes = { "Exception", "Server", "ThirdParty", "Other" };
            string[] errorDetails = { "Exception in Code", "Server", "Third Party like Email Sender API", "Other Errors" };

            for (int i = 0; i < errorTypes.Length; i++)
            {
                var type = new ErrorType
                {
                    Name = errorTypes[i],
                    Details = errorDetails[i]
                };
                await context.ErrorType.AddAsync(type);
                await context.SaveChangesAsync();
            }
        }


        static string[] LanguageCodeId = { "en", "ar", "de", "fr", "it", "es", "nl", "sv", "tr" };
        public static async Task AddDefaultLanguages(IServiceProvider services)
        {
            var context = services.GetRequiredService<AppDbContext>();

            string[] Languages = { "English", "العربية", "Deutsche", "Français", "Italiana", "español", "Nederlands", "svenska", "Türk" };
            string[] LangDir = { "ltr", "rtl", "ltr", "ltr", "ltr", "ltr", "ltr", "ltr", "ltr" };

            for (int i = 0; i < Languages.Length; i++)
            {
                var lan = new Language
                {
                    CodeId = LanguageCodeId[i],
                    Name = Languages[i],
                    LanguageDirection = LangDir[i],
                };
                await context.Language.AddAsync(lan);
            }
            await context.SaveChangesAsync();
        }

        public static async Task AddDefaultBlogCategories(IServiceProvider services)
        {
            var context = services.GetRequiredService<AppDbContext>();


            // first add the BlogCategoryName to be the parent of all BlogCategory in any languages
            string[] BlogCategoryNames = { "Food", "Travel", "Music", "Lifestyle", "Fitness", "Sports"};
            int[] BlogCategoryNamesId = { 1, 2, 3, 4,5, 6};

            for (int i = 0; i < BlogCategoryNames.Length; i++)
            {
                var name = new BlogSourceCategoryName
                {
                    Id = BlogCategoryNamesId[i],
                    Name = BlogCategoryNames[i]
                };
                await context.BlogSourceCategoryName.AddAsync(name);
            }
            await context.SaveChangesAsync();


            string[] BlogCategoryNamesEn = { "Food", "Travel", "Music", "Lifestyle", "Fitness", "Sports" };
            string[] BlogCategoryNamesDe = { "Essen", "Reise", "Musik", "Lebensstil", "Fitness", "Sport" };
            string[] BlogCategoryNamesAr = { "طعام", "السفر", "موسيقى", "أسلوب الحياة", "اللياقه البدنيه", "رياضة" };

            for (int i = 0; i < BlogCategoryNames.Length; i++)
            {
                // En
                var lEn = new BlogCategory
                {
                    Name = BlogCategoryNamesEn[i],
                    SourceCategoryId = BlogCategoryNamesId[i],
                    LanguageId = LanguageCodeId[2]
                };

                // De
                var lDe = new BlogCategory
                {
                    Name = BlogCategoryNamesDe[i],
                    SourceCategoryId = BlogCategoryNamesId[i],
                    LanguageId = LanguageCodeId[2]
                };
                await context.BlogCategory.AddAsync(lDe);

                // Ar
                var lAr = new BlogCategory
                {
                    Name = BlogCategoryNamesAr[i],
                    SourceCategoryId = BlogCategoryNamesId[i],
                    LanguageId = LanguageCodeId[1]
                };
                await context.BlogCategory.AddAsync(lAr);

            }
            await context.SaveChangesAsync();
        }

        /*
        this class to create list of these Ids to use object 
        in add add blogs images*/
        public class BlogUploads
        {
            public int BlogId { get; set; }
            public int UploadId { get; set; }
        }

        public static async Task AddSeedBlogs(IServiceProvider services)
        {

            var context = services.GetRequiredService<AppDbContext>();

            Random random = new Random();

            int[] blogCat = { 1,1,1, 2,2, 3,3,3, 6,6, 5,5,5, 4,4,4,4,4,4,4 };

            string[] bodyAr =
            {
                "طبق شهي و لذيذ شكله جميل يمكن تحضيره من الخضار المتواجدة دائما في المطبخ العربي",
                " المقبلات أو المشهيات أو المزة (في الشام) أو المفتحات (في تونس) هي مادة غذائية من الخضار أو الفاكهة تكون مطبوخا أو مخللا أو مفرومًا وعادة ما يُستخدم كـتوابل خصوصًا لتجميل الطبق الرئيسي. ",
                "المخلل من التخليل (خلّله) أي وضعه في مادة تخلّله فالخيار مثلاً ينقع في الماء والملح في إناء مضغوط ومسحوب منه الهواء لفترة أسبوع",
                
                "ختبر الإنسانُ السَّفر منذ بداية وجوده على المعمورة، حيث كان مضطرًّا للتنقُّل بحثًا عن الماء والطَّعام، ثمَّ استمرَّ في ذلك بعد أنْ أصبح في غنيةٍ عن مشقَّة الطَّريق، فقد فطرتِ النَّفس البشريَّة",
                "إيجابيات السَّفر وسلبياته والمشاعر يحكمها طبع الإنسان من جهةٍ وطبيعة سفره وحاله من جهةٍ أخرى؛ فمن خبر محاسن الغربة ووجد فيها ملاذًا مطمئنًا له",
                
                "فيروز مغنية لبنانية. اسمها الحقيقي «نهاد رزق وديع حداد»، ولدت في نوفمبر من سنة 1935 في حي يسمى 《زقاق البلاط》 في بيروت، قدمت العديد من الأغاني والأوبريهات",
                "ليلى مراد مغنية وممثلة مصرية، تعتبر من أبرز المغنيات والممثلات في الوطن العربي في القرن العشرين. بدأت مشوارها مع الغناء في سن أربعة عشر عامًا",
                "سيد درويش مغني وملحن مصري اسمه الحقيقي السيد درويش البحر هو مجدد الموسيقى وباعث النهضة[؟] الموسيقية في مصر والوطن العربي. بدأ ينشد مع أصدقائه ألحان الشيخ",
                
                "لألعاب الأولمبية الصيفية ، أو ألعاب الأولمبياد الصيفية ، هي حدث رياضي يقام كل أربع سنوات لممارسة الألعاب الأولمبية",
                " ليبرون جيمس حل أولاً بين لاعبي NBA وفي المركز الثاني عامة بعد كريستيانو رونالدو نجم الريال الذي تصدّر القائمة. في حين حلّ كيفن دورانت في المركز الثامن.",

                "وعرف اللياقة البدنية Clarke، 1976 على أنها القدرة على القيام بالأعباء اليومية بقوة ووعي وبدون تعب لا مبرر له من توافر قدر كاف ممن الطاقة",
                "هي السمات البدنية التي على الفرد تحقيقها للقيام بالأعمال اليومية الاعتيادية بأفضل كفاءة دون الشعور بالتعب والإرهاق الشديد. هي الأعمال اليومية على بساطتها، لكنها قد تزداد تعقيدا لتشمل الرياضات التنافسية والنشاطات الأكثر صعوبة.",
                "المرونة هي قدرة المفصل على الحركة لأبعد مدى ضمن نطاق حركته! ومن الأمثلة على التمارين التي تحسن المرونة تمارين اليوغا وشد الجسم. مثلاً، قم بوضع قدميك على الأرض مع جعل مسافة بينهما",

                "المِرِّيخ أو الكوكب الأحمر هو الكوكب الرابع من حيث البعد عن الشمس في النظام الشمسي وهو الجار الخارجي للأرض ويصنف كوكبا صخريا",
                "مذبحة القلعة، أو مذبحة المماليك هي حادثة تاريخيَّة وقعت في ولاية مصر العثمانية دبرها محمد علي باشا للتخلص من أعدائه المماليك يوم الجمعة 5 صفر سنة 1226 هـ الموافق 1 مارس",
                "اعتقلت الولايات المتحدة الأمريكية أكثر من 779 معتقلاً إدارياً خارج نطاق القضاء في معتقل غوانتانامو الأمريكي في كوبا منذ فتح معسكرات الاعتقال في 11 يناير 2002",
                "الحركة الثقافية هي كل تغيير في طريقة التفكير والاعتقادات السائدة بخصوص مجموعة من القضايا الثقافية ذات العلاقة بنواحٍ وفروع معينة، مما يترتب عليه تغيير في طريقة مقاربة ومعالجة العمل",
                "الهندسة التطبيقية تعني تطبيق العلم لتوفير الحاجات الإنسانية. وذلك من خلال تطبيق العلوم النظرية والتطبيقية: الفيزياء، الكيمياء، الرياضيات، والأحياء.",
                "الإنسان الآلي أوالروبوت (بالإنجليزية: Robot)‏ عبارة عن أداة ميكانيكية قادرة على القيام بفعاليات مبرمجة سلفا ويقوم الإنسان الآلي بإنجاز تلك الفعاليات",
                "إمحوتب هو بانى هرم زوسر المدرج وهو أول مهندس معمارى ، وأحد أشهر المهندسين في مصر القديمة ، رفع إلى درجة معبود بعد وفاتة وأصبح إله الطب ."
            };

            string[] bodyDe =
            {
                "Ein köstliches und köstliches Gericht mit einer schönen Form, das aus dem Gemüse zubereitet werden kann, das in der arabischen Küche immer vorhanden ist",
                "Vorspeisen, Vorspeisen, Mezze (in der Levante) oder Moftahat (in Tunesien) ist ein Lebensmittel aus Gemüse oder Obst, das gekocht, eingelegt oder gehackt wird. Es wird normalerweise als Gewürz verwendet, insbesondere um das Hauptgericht zu verschönern.",
                "Die Gurke wird aus Beizen (Beizen) hergestellt, dh in eine Substanz gegeben, die damit gemischt ist. Die Gurke wird beispielsweise in einem Druckbehälter, der eine Woche lang Luft daraus zieht, in Wasser und Salz eingeweicht.",

                "Der Mensch erlebte Reisen von Beginn seiner Existenz auf dem Planeten an, als er gezwungen war, sich auf der Suche nach Wasser und Nahrung zu bewegen, und fuhr dann damit fort, nachdem er von den Strapazen der Straße reich geworden war, als die menschliche Seele unerträglich wurde.",
                "Die positiven und negativen Aspekte des Reisens und der Gefühle werden einerseits von der Natur eines Menschen und andererseits von der Art seiner Reise und seinem Zustand bestimmt. Aus den Nachrichten über die Vorteile der Entfremdung fand er darin einen beruhigenden Zufluchtsort für ihn.",

                "Fairouz ist eine libanesische Sängerin. Ihr richtiger Name ist  Nihad Rizk Wadih Haddad  . Sie wurde im November 1935 in einem Viertel namens Zuqaq El Blat in Beirut geboren. Sie spielte viele Lieder und Opern.",
                "Laila Murad ist eine ägyptische Sängerin und Schauspielerin. Sie gilt im 20. Jahrhundert als eine der bekanntesten Sängerinnen und Schauspielerinnen der arabischen Welt. Sie begann ihre Gesangskarriere im Alter von vierzehn Jahren.",
                "Sayed Darwish ist ein ägyptischer Sänger und Komponist, dessen richtiger Name Herr Darwish Al-Bahr ist. Er ist der Erneuerer der Musik und der Katalysator für die musikalische Renaissance [?] In Ägypten und der arabischen Welt.",

                "Die Olympischen Sommerspiele oder die Olympischen Sommerspiele sind ein Sportereignis, das alle vier Jahre für die Olympischen Spiele stattfindet.",
                "LeBron James wurde Erster unter den NBA-Spielern und im Allgemeinen Zweiter nach Cristiano Ronaldo, dem Star von Real Madrid, der die Liste anführte, während Kevin Durant Achter wurde.",

                "Er definierte Fitness von Clarke, 1976, als die Fähigkeit, tägliche Aufgaben mit Kraft, Bewusstsein und ohne übermäßige Müdigkeit aufgrund der Verfügbarkeit ausreichender Energie auszuführen.",
                "Sie sind die physischen Merkmale, die ein Individuum erreichen muss, um normale tägliche Aufgaben mit der besten Effizienz zu erledigen, ohne sich müde und erschöpft zu fühlen. Die täglichen Aufgaben sind einfach, aber sie können komplexer werden, um Leistungssport und schwierigere Aktivitäten einzuschließen.",
                "Flexibilität ist die Fähigkeit des Gelenks, sich innerhalb seines Bewegungsbereichs am weitesten zu bewegen! Beispiele für Übungen, die die Flexibilität verbessern, sind Yoga-Übungen, die den Körper dehnen. Stellen Sie beispielsweise Ihre Füße mit einem Abstand zwischen ihnen auf den Boden.",

                "Der Mars oder der rote Planet ist der vierte Planet in Bezug auf die Entfernung von der Sonne im Sonnensystem und der äußerste Nachbar der Erde. Er wird als felsiger Planet klassifiziert.",
                "Das Massaker an der Burg oder das Massaker an den Mamluken ist ein historischer Vorfall in der osmanischen Provinz Ägypten, der von Muhammad Ali Pascha geplant wurde, um seine Mamluk-Feinde am Freitag, dem 5. Safar 1226 AH, entsprechend dem 1. März, loszuwerden.",
                "Die Vereinigten Staaten von Amerika haben seit der Eröffnung der Haftlager am 11. Januar 2002 mehr als 779 außergerichtliche Verwaltungshäftlinge im Haftzentrum von Guantanamo Bay in Kuba festgenommen.",
                "Die kulturelle Bewegung ist jede Änderung der Denkweise und der vorherrschenden Überzeugungen in Bezug auf eine Gruppe kultureller Probleme, die sich auf bestimmte Aspekte und Zweige beziehen, was zu einer Änderung der Art und Weise führt, wie wir mit Arbeit umgehen und sie behandeln.",
                "Angewandte Technik bedeutet, Wissenschaft anzuwenden, um die Bedürfnisse des Menschen durch Anwendung theoretischer und angewandter Wissenschaften zu erfüllen: Physik, Chemie, Mathematik und Biologie.",
                "Ein Roboter oder ein Roboter (auf Englisch: Roboter) ist ein mechanisches Werkzeug, das vorprogrammierte Aktivitäten ausführen kann, und der Roboter führt diese Aktivitäten aus.",
                "Imhotep war der Erbauer der Stufenpyramide von Djoser, dem ersten Architekten und einer der berühmtesten Ingenieure im alten Ägypten. Nach seinem Tod wurde er zum Idol erhoben und zum Gott der Medizin."
            };

            var categiryList = new List<int>();
            // 1000*20=20000 Blogs
            for (int r = 0; r < 1000; r++)
            {

                for (var i = 0; i < bodyAr.Length; i++)
                {
                    var title = bodyDe[i].Substring(0, 15);
                    var sTitle = bodyDe[i].Substring(0, 30);
                    var body = bodyDe[i];
                    var lngId = LanguageCodeId[2];
                    var publiched = true;
                    var top = false;

                    if (i % 2 == 0)
                    {
                        title = bodyAr[i].Substring(0, 15);
                        sTitle = bodyAr[i].Substring(0, 30);
                        body = bodyAr[i];
                        lngId = LanguageCodeId[1];
                    }

                    if (i % 3 == 1)
                        publiched = false;
                    // to add some of blogs  of Top of the list
                    if ((r == 10 && i == 15) || (r == 15 && i == 6) || (r == 30 && i == 18))
                        top = true;


                    var daterelase = DateTime.Now;
                    var randNum = random.Next(1, 100);
                    if (randNum == 1)
                        daterelase = DateTime.Now;
                    else if (randNum > 1 && randNum < 10)
                        daterelase = DateTime.Now.AddDays(-5);
                    else if (randNum > 10 && randNum < 20)
                        daterelase = DateTime.Now.AddDays(-10);
                    else if (randNum > 20 && randNum < 30)
                        daterelase = DateTime.Now.AddDays(-15);
                    else if (randNum > 30 && randNum < 40)
                        daterelase = DateTime.Now.AddDays(-20);
                    else if (randNum > 40 && randNum < 50)
                        daterelase = DateTime.Now.AddDays(-25);
                    else if (randNum > 50 && randNum < 60)
                        daterelase = DateTime.Now.AddDays(-30);
                    else if (randNum > 60 && randNum < 70)
                        daterelase = DateTime.Now.AddDays(-35);
                    else if (randNum > 70 && randNum < 80)
                        daterelase = DateTime.Now.AddDays(-40);
                    else if (randNum > 80 && randNum < 90)
                        daterelase = DateTime.Now.AddDays(-45);


                    // add new Blog
                    var newBlog = new Blog
                    {
                        Title = title,
                        ShortTitle = sTitle,
                        Body = body,
                        Publish = publiched,
                        Commentable = true,
                        AtTop = top,
                        ReleaseDate = daterelase,
                        LanguageId = lngId,
                        UserId = superAdmin_id,
                        AddedDateTime = DateTime.Now
                    };
                    categiryList.Add(blogCat[i]);

                    context.Blog.Add(newBlog);
                }
            }
            await context.SaveChangesAsync();

            // get just the last 10000 blog to add categoryies and images to save time
            var blogs = context.Blog.Where(b=>b.Publish == true).OrderByDescending(b => b.ReleaseDate).Take(10000).ToList();

            // Add Blogs categories
            foreach (var blog in blogs)
            {
                var catId = random.Next(0, 7);
                var cat = new BlogCategoryList
                {
                    BlogId = blog.Id,
                    BlogCategoryId = categiryList[catId]
                };
                context.BlogCategoryList.Add(cat);

            }
            //await context.SaveChangesAsync();

            // Add Blogs Images, every blog has from 1 to 3 images
            var blogUpload = new List<BlogUploads>();
            foreach (var blog in blogs)
            {
                var indexImage = random.Next(1, 52);
                var repNumber = random.Next(1, 4);
                for (int rep = 1; rep <= repNumber; rep++)
                {
                    var upload = new Upload
                    {
                        Name = "img (" + indexImage + "_" + rep + ")",
                        Path = "/Uploads/Images/img (" + indexImage + ").jfif",
                        AddedDateTime = DateTime.Now,
                        UserId = superAdmin_id
                    };
                    await context.Upload.AddAsync(upload);
                    var saveBlogUplad = new BlogUploads
                    {
                        BlogId = blog.Id,
                        UploadId = upload.Id
                    };
                    blogUpload.Add(saveBlogUplad);
                }
            }
            await context.SaveChangesAsync();

            // add images data in UploadBlogImagesList table
            var defaultImage = true;
            int blogId = 0;
            foreach (var item in blogUpload)
            {
                if (item.BlogId != blogId)
                    defaultImage = true;

                var blogImage = new UploadBlogImagesList
                {
                    UploadId = item.UploadId,
                    BlogId = item.BlogId,
                    Default = defaultImage,
                    UploadTypeId = 3
                };
                blogId = item.BlogId;
                defaultImage = false;

                await context.UploadBlogImagesList.AddAsync(blogImage);
            }
            await context.SaveChangesAsync();


            // add some Like and dislike for Blogs
            var users = context.Users.ToList();

            blogs = blogs.ToList();
            foreach (var user in users)
            {
                // like/dislike number for this user
                var repeatedNum = random.Next(1, blogs.Count());
                // to be sure the blog id is not repeated for every user
                var blogIdsList = new List<int>();
                for (int i = 0; i < repeatedNum; i++)
                {
                    var like = true;
                    var dislike = false;
                    // select random Blog
                    var randomBlog = blogs[random.Next(1, blogs.Count())];
                    // if this blog not selected before
                    if (!blogIdsList.Contains(randomBlog.Id))
                    {
                        if (random.Next(1, 5) < 2)
                        {
                            like = false;
                            dislike = true;
                        }
                        var likeDislike = new BlogLike
                        {
                            Like = like,
                            Dislike = dislike,
                            AddedDateTime = DateTime.Now,
                            BlogId = randomBlog.Id,
                            UserId = user.Id
                        };
                        blogIdsList.Add(randomBlog.Id);
                        await context.BlogLike.AddAsync(likeDislike);

                        var comment = new BlogComment
                        {
                            AddedDateTime = DateTime.Now,
                            BlogId = randomBlog.Id,
                            UserId = user.Id,
                            Comment = "Hi I'm "+user.UserName+", random number : "+ i
                        };
                        await context.BlogComment.AddAsync(comment);
                    }
                }
            }
            await context.SaveChangesAsync();
        }



        public static async Task AddSeedUsersData(IServiceProvider services)
        {
            // Get UserManager Service to Application
            UserManager<AppUser> userManager = services.GetRequiredService<UserManager<AppUser>>();
            var context = services.GetRequiredService<AppDbContext>();
            Random random = new Random();

            // create Users list
            for (int i = 1; i <= 100; i++)
            {
                var Confirmed = true;
                var name = "visitor";
                var role = "Visitor";
                var AuPass = false;
                var lang = "de,";

                if (i >= 20 && i < 25)
                {
                    AuPass = true;
                }

                if (i >= 25 && i < 30)
                {
                    name = "admin";
                    role = "Admin";
                }
                if (i >= 30 && i < 35)
                {
                    name = "editor";
                    role = "Editor";
                }
                if (i >= 40 && i < 50 )
                {
                    Confirmed = false;
                }

                if (i % 2 == 0)
                    lang = "de,ar,";

                if (i % 7 == 0)
                    lang = "ar,";

                if (i % 8 == 1)
                    lang = "en,fr,";

                var userS = new AppUser
                {
                    Email = name + "_" + i + "@al-alloush.com",
                    EmailConfirmed = Confirmed,
                    UserName = name + "_" + i,
                    FirstName = "First " + name + " Name",
                    LastName = "Last " + name + " Name",
                    SelectedLanguages = lang,
                    RegisterDate = DateTime.Now,
                    Birthday = DateTime.Parse("15/6/1978", MyCultureInfo),
                    PhoneNumber = "17328547" + i,
                    PhoneNumberConfirmed = Confirmed,
                    AutomatedPassword = AuPass,
                    Address = new Address
                    {
                        Street = "street_" + i,
                        BuildingNum = "12",
                        Flore = "L 3",
                        City = "city_" + i,
                        Zipcode = "874" + i,
                        Country = "German"
                    }
                };
                IdentityResult createS = await userManager.CreateAsync(userS, "!QA1qa");
                if (createS.Succeeded)
                    userManager.AddToRoleAsync(userS, role).Wait();
            }

            var users = await context.Users.ToArrayAsync();
            for (int i = 1; i < users.Length; i++)
            {
                var imgNum = random.Next(1, 52);
                var upload = new Upload
                {
                    Name = "user (" + imgNum + "): " + random.Next(1, 100000),
                    Path = "/Uploads/Images/user (" + imgNum + ").jpg",
                    AddedDateTime = DateTime.Now,
                    UserId = users[i].Id
                };
                await context.Upload.AddAsync(upload);
                await context.SaveChangesAsync();
            }
           
            for (int i = 1; i < users.Length; i++)
            {
                var userUploads = await context.Upload.FirstOrDefaultAsync(u => u.UserId == users[i].Id);

                var userImage = new UploadUserImagesList
                {
                    UploadId = userUploads.Id,
                    UserId = users[i].Id,
                    Default = true,
                    UploadTypeId = 1
                };
                await context.UploadUserImagesList.AddAsync(userImage);
                await context.SaveChangesAsync();
            }
        }


    }
}
