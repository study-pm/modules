USE [master]
GO
/****** Object:  Database [CookingBook]    Script Date: 28.10.2024 20:30:27 ******/
CREATE DATABASE [CookingBook]
 CONTAINMENT = NONE 
GO
ALTER DATABASE [CookingBook] SET COMPATIBILITY_LEVEL = 150
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [CookingBook].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [CookingBook] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [CookingBook] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [CookingBook] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [CookingBook] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [CookingBook] SET ARITHABORT OFF 
GO
ALTER DATABASE [CookingBook] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [CookingBook] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [CookingBook] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [CookingBook] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [CookingBook] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [CookingBook] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [CookingBook] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [CookingBook] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [CookingBook] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [CookingBook] SET  DISABLE_BROKER 
GO
ALTER DATABASE [CookingBook] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [CookingBook] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [CookingBook] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [CookingBook] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [CookingBook] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [CookingBook] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [CookingBook] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [CookingBook] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [CookingBook] SET  MULTI_USER 
GO
ALTER DATABASE [CookingBook] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [CookingBook] SET DB_CHAINING OFF 
GO
ALTER DATABASE [CookingBook] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [CookingBook] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [CookingBook] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [CookingBook] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [CookingBook] SET QUERY_STORE = OFF
GO
USE [CookingBook]
GO
/****** Object:  Table [dbo].[Authors]    Script Date: 28.10.2024 20:30:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Authors](
	[AuthorID] [int] IDENTITY(1,1) NOT NULL,
	[AuthorName] [nvarchar](100) NOT NULL,
	[Login] [nvarchar](100) NOT NULL,
	[Password] [nvarchar](50) NOT NULL,
	[Birthdate] [datetime] NULL,
	[Phone] [nchar](10) NULL,
	[Email] [nvarchar](50) NULL,
	[Experience] [int] NULL,
 CONSTRAINT [PK_Authors] PRIMARY KEY CLUSTERED 
(
	[AuthorID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Categories]    Script Date: 28.10.2024 20:30:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Categories](
	[CategoryID] [int] IDENTITY(1,1) NOT NULL,
	[CategoryName] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_Categories] PRIMARY KEY CLUSTERED 
(
	[CategoryID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CookingSteps]    Script Date: 28.10.2024 20:30:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CookingSteps](
	[StepID] [int] IDENTITY(1,1) NOT NULL,
	[RecipeID] [int] NOT NULL,
	[StepNumber] [int] NOT NULL,
	[StepDescription] [nvarchar](max) NULL,
 CONSTRAINT [PK_CookingSteps] PRIMARY KEY CLUSTERED 
(
	[StepID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Ingredients]    Script Date: 28.10.2024 20:30:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Ingredients](
	[IngredientID] [int] IDENTITY(1,1) NOT NULL,
	[IngredientName] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_Ingredients] PRIMARY KEY CLUSTERED 
(
	[IngredientID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecipeIngredients]    Script Date: 28.10.2024 20:30:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecipeIngredients](
	[RecipeIngredientID] [int] IDENTITY(1,1) NOT NULL,
	[RecipeID] [int] NOT NULL,
	[IngredientID] [int] NOT NULL,
	[Quantity] [int] NOT NULL,
 CONSTRAINT [PK_RecipeIngredients] PRIMARY KEY CLUSTERED 
(
	[RecipeIngredientID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Recipes]    Script Date: 28.10.2024 20:30:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Recipes](
	[RecipeID] [int] IDENTITY(1,1) NOT NULL,
	[RecipeName] [nvarchar](200) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[CategoryID] [int] NULL,
	[AuthorID] [int] NULL,
	[CookingTime] [int] NULL,
	[Image] [nvarchar](50) NULL,
 CONSTRAINT [PK_Recipes] PRIMARY KEY CLUSTERED 
(
	[RecipeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecipeTags]    Script Date: 28.10.2024 20:30:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecipeTags](
	[RecipeTagID] [int] IDENTITY(1,1) NOT NULL,
	[RecipeID] [int] NOT NULL,
	[TagID] [int] NOT NULL,
 CONSTRAINT [PK_RecipeTags] PRIMARY KEY CLUSTERED 
(
	[RecipeTagID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Reviews]    Script Date: 28.10.2024 20:30:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Reviews](
	[ReviewID] [int] IDENTITY(1,1) NOT NULL,
	[RecipeID] [int] NOT NULL,
	[ReviewText] [nvarchar](max) NULL,
	[Rating] [int] NOT NULL,
 CONSTRAINT [PK_Reviews] PRIMARY KEY CLUSTERED 
(
	[ReviewID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Tags]    Script Date: 28.10.2024 20:30:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tags](
	[TagID] [int] IDENTITY(1,1) NOT NULL,
	[TagName] [int] NOT NULL,
 CONSTRAINT [PK_Tags] PRIMARY KEY CLUSTERED 
(
	[TagID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[Authors] ON 

INSERT [dbo].[Authors] ([AuthorID], [AuthorName], [Login], [Password]) VALUES (1, N'Стрелков Макар Вениаминович', N'strelok', N'123saw')
INSERT [dbo].[Authors] ([AuthorID], [AuthorName], [Login], [Password]) VALUES (2, N'Дроздов Севастьян Николаевич', N'drozed', N'qwxx')
INSERT [dbo].[Authors] ([AuthorID], [AuthorName], [Login], [Password]) VALUES (3, N'Галкин Платон Авксентьевич', N'gqql', N'qwe662')
INSERT [dbo].[Authors] ([AuthorID], [AuthorName], [Login], [Password]) VALUES (4, N'Попов Моисей Демьянович', N'popov', N'qooWasf')
INSERT [dbo].[Authors] ([AuthorID], [AuthorName], [Login], [Password]) VALUES (5, N'Титов Прохор Геласьевич', N'Qwool', N'asf789')
INSERT [dbo].[Authors] ([AuthorID], [AuthorName], [Login], [Password]) VALUES (6, N'Петухова Милана Романовна', N'pet', N'poow78')
INSERT [dbo].[Authors] ([AuthorID], [AuthorName], [Login], [Password]) VALUES (7, N'Самсонова Милана Ефимовна', N'SooiI', N'asf789')
INSERT [dbo].[Authors] ([AuthorID], [AuthorName], [Login], [Password]) VALUES (8, N'Федосеева Георгина Данииловна', N'phooe', N'789')
INSERT [dbo].[Authors] ([AuthorID], [AuthorName], [Login], [Password]) VALUES (9, N'Иванкова Грета Владленовна', N'Ivan', N'wojoOi')
INSERT [dbo].[Authors] ([AuthorID], [AuthorName], [Login], [Password]) VALUES (10, N'Белякова Лира Мэлсовна', N'Belyak', N'blloe98')
SET IDENTITY_INSERT [dbo].[Authors] OFF
GO
SET IDENTITY_INSERT [dbo].[Categories] ON 

INSERT [dbo].[Categories] ([CategoryID], [CategoryName]) VALUES (1, N'основное блюдо')
INSERT [dbo].[Categories] ([CategoryID], [CategoryName]) VALUES (2, N'гарнир')
INSERT [dbo].[Categories] ([CategoryID], [CategoryName]) VALUES (3, N'десерт')
INSERT [dbo].[Categories] ([CategoryID], [CategoryName]) VALUES (4, N'напиток')
INSERT [dbo].[Categories] ([CategoryID], [CategoryName]) VALUES (5, N'аперитив')
INSERT [dbo].[Categories] ([CategoryID], [CategoryName]) VALUES (6, N'дайджестив')
INSERT [dbo].[Categories] ([CategoryID], [CategoryName]) VALUES (7, N'закуска')
SET IDENTITY_INSERT [dbo].[Categories] OFF
GO
SET IDENTITY_INSERT [dbo].[CookingSteps] ON 

INSERT [dbo].[CookingSteps] ([StepID], [RecipeID], [StepNumber], [StepDescription]) VALUES (1, 1, 1, N'Подготовьте необходимые ингредиенты.
С консервированных ананасов слейте сок.
Включите духовку для разогрева до 180 градусов.')
INSERT [dbo].[CookingSteps] ([StepID], [RecipeID], [StepNumber], [StepDescription]) VALUES (2, 1, 2, N'Куриное филе хорошо промойте и обсушите бумажным полотенцем. Разрежьте каждое филе вдоль пополам, не прорезая до конца, и разверните как книжку.')
INSERT [dbo].[CookingSteps] ([StepID], [RecipeID], [StepNumber], [StepDescription]) VALUES (3, 1, 3, N'Посыпьте подготовленное куриное филе солью, чёрным перцем и 1/2 ч. ложки карри. Хорошо натрите каждое филе со всех сторон и внутри.')
INSERT [dbo].[CookingSteps] ([StepID], [RecipeID], [StepNumber], [StepDescription]) VALUES (4, 1, 4, N'Сладкий перец очистите.
Нарежьте ананасы и перец некрупными кусочками.')
INSERT [dbo].[CookingSteps] ([StepID], [RecipeID], [StepNumber], [StepDescription]) VALUES (5, 1, 5, N'Из 1/4 лимона отожмите 1 ст. ложку сока.
Ананасы и перец переложите в миску. Добавьте растительное масло, лимонный сок и 1/2 ч. ложки карри. Всё перемешайте.')
INSERT [dbo].[CookingSteps] ([StepID], [RecipeID], [StepNumber], [StepDescription]) VALUES (6, 1, 6, N'Полученной смесью начините кармашки.')
INSERT [dbo].[CookingSteps] ([StepID], [RecipeID], [StepNumber], [StepDescription]) VALUES (7, 1, 7, N'Куриное филе с начинкой выложите на лист пергамента и заверните как конфету.')
INSERT [dbo].[CookingSteps] ([StepID], [RecipeID], [StepNumber], [StepDescription]) VALUES (8, 1, 8, N'
Выложите заготовки в форму для запекания. Отправьте в предварительно разогретую до 180 градусов духовку на 40 минут, до готовности (при прокалывании куриного мяса должен выделяться светлый сок).')
INSERT [dbo].[CookingSteps] ([StepID], [RecipeID], [StepNumber], [StepDescription]) VALUES (9, 1, 9, N'Аккуратно разверните пергамент, переложите курицу на тарелку.')
INSERT [dbo].[CookingSteps] ([StepID], [RecipeID], [StepNumber], [StepDescription]) VALUES (10, 1, 10, N'Куриное филе, запечённое с ананасами и сладким перцем (в пергаменте), можно подавать к столу.')
SET IDENTITY_INSERT [dbo].[CookingSteps] OFF
GO
SET IDENTITY_INSERT [dbo].[Ingredients] ON 

INSERT [dbo].[Ingredients] ([IngredientID], [IngredientName]) VALUES (1, N'куриное филе')
INSERT [dbo].[Ingredients] ([IngredientID], [IngredientName]) VALUES (2, N'ананасы консервированные')
INSERT [dbo].[Ingredients] ([IngredientID], [IngredientName]) VALUES (3, N'перец сладкий красный')
INSERT [dbo].[Ingredients] ([IngredientID], [IngredientName]) VALUES (4, N'карри')
INSERT [dbo].[Ingredients] ([IngredientID], [IngredientName]) VALUES (5, N'соль')
INSERT [dbo].[Ingredients] ([IngredientID], [IngredientName]) VALUES (6, N'перец черный молотый')
INSERT [dbo].[Ingredients] ([IngredientID], [IngredientName]) VALUES (7, N'масло растительное')
INSERT [dbo].[Ingredients] ([IngredientID], [IngredientName]) VALUES (8, N'лимон')
SET IDENTITY_INSERT [dbo].[Ingredients] OFF
GO
SET IDENTITY_INSERT [dbo].[RecipeIngredients] ON 

INSERT [dbo].[RecipeIngredients] ([RecipeIngredientID], [RecipeID], [IngredientID], [Quantity]) VALUES (1, 1, 1, 700)
INSERT [dbo].[RecipeIngredients] ([RecipeIngredientID], [RecipeID], [IngredientID], [Quantity]) VALUES (2, 1, 3, 170)
INSERT [dbo].[RecipeIngredients] ([RecipeIngredientID], [RecipeID], [IngredientID], [Quantity]) VALUES (3, 1, 2, 240)
INSERT [dbo].[RecipeIngredients] ([RecipeIngredientID], [RecipeID], [IngredientID], [Quantity]) VALUES (4, 1, 4, 5)
INSERT [dbo].[RecipeIngredients] ([RecipeIngredientID], [RecipeID], [IngredientID], [Quantity]) VALUES (5, 1, 5, 2)
INSERT [dbo].[RecipeIngredients] ([RecipeIngredientID], [RecipeID], [IngredientID], [Quantity]) VALUES (6, 1, 6, 1)
INSERT [dbo].[RecipeIngredients] ([RecipeIngredientID], [RecipeID], [IngredientID], [Quantity]) VALUES (7, 1, 7, 30)
INSERT [dbo].[RecipeIngredients] ([RecipeIngredientID], [RecipeID], [IngredientID], [Quantity]) VALUES (8, 1, 1, 1)
SET IDENTITY_INSERT [dbo].[RecipeIngredients] OFF
GO
SET IDENTITY_INSERT [dbo].[Recipes] ON 

INSERT [dbo].[Recipes] ([RecipeID], [RecipeName], [Description], [CategoryID], [AuthorID], [CookingTime], [Image]) VALUES (1, N'Куриное филе, запечённое с ананасами и сладким перцем', N'сочная куриная грудка к праздничному столу. Кармашки из филе фаршируются консервированными ананасами и болгарским перцем, и затем заворачиваются в пергамент. При запекании все соки сохраняются внутри, куриное мясо получается нежным, приобретает изысканный сладковатый вкус.', 1, 1, 55, N'big_703435')
SET IDENTITY_INSERT [dbo].[Recipes] OFF
GO
ALTER TABLE [dbo].[CookingSteps]  WITH CHECK ADD  CONSTRAINT [FK_CookingSteps_Recipes] FOREIGN KEY([RecipeID])
REFERENCES [dbo].[Recipes] ([RecipeID])
GO
ALTER TABLE [dbo].[CookingSteps] CHECK CONSTRAINT [FK_CookingSteps_Recipes]
GO
ALTER TABLE [dbo].[RecipeIngredients]  WITH CHECK ADD  CONSTRAINT [FK_RecipeIngredients_Ingredients] FOREIGN KEY([IngredientID])
REFERENCES [dbo].[Ingredients] ([IngredientID])
GO
ALTER TABLE [dbo].[RecipeIngredients] CHECK CONSTRAINT [FK_RecipeIngredients_Ingredients]
GO
ALTER TABLE [dbo].[RecipeIngredients]  WITH CHECK ADD  CONSTRAINT [FK_RecipeIngredients_Recipes] FOREIGN KEY([RecipeID])
REFERENCES [dbo].[Recipes] ([RecipeID])
GO
ALTER TABLE [dbo].[RecipeIngredients] CHECK CONSTRAINT [FK_RecipeIngredients_Recipes]
GO
ALTER TABLE [dbo].[Recipes]  WITH CHECK ADD  CONSTRAINT [FK_Recipes_Authors] FOREIGN KEY([AuthorID])
REFERENCES [dbo].[Authors] ([AuthorID])
GO
ALTER TABLE [dbo].[Recipes] CHECK CONSTRAINT [FK_Recipes_Authors]
GO
ALTER TABLE [dbo].[Recipes]  WITH CHECK ADD  CONSTRAINT [FK_Recipes_Categories1] FOREIGN KEY([CategoryID])
REFERENCES [dbo].[Categories] ([CategoryID])
GO
ALTER TABLE [dbo].[Recipes] CHECK CONSTRAINT [FK_Recipes_Categories1]
GO
ALTER TABLE [dbo].[RecipeTags]  WITH CHECK ADD  CONSTRAINT [FK_RecipeTags_Recipes] FOREIGN KEY([RecipeID])
REFERENCES [dbo].[Recipes] ([RecipeID])
GO
ALTER TABLE [dbo].[RecipeTags] CHECK CONSTRAINT [FK_RecipeTags_Recipes]
GO
ALTER TABLE [dbo].[RecipeTags]  WITH CHECK ADD  CONSTRAINT [FK_RecipeTags_Tags] FOREIGN KEY([TagID])
REFERENCES [dbo].[Tags] ([TagID])
GO
ALTER TABLE [dbo].[RecipeTags] CHECK CONSTRAINT [FK_RecipeTags_Tags]
GO
ALTER TABLE [dbo].[Reviews]  WITH CHECK ADD  CONSTRAINT [FK_Reviews_Recipes] FOREIGN KEY([RecipeID])
REFERENCES [dbo].[Recipes] ([RecipeID])
GO
ALTER TABLE [dbo].[Reviews] CHECK CONSTRAINT [FK_Reviews_Recipes]
GO
USE [master]
GO
ALTER DATABASE [CookingBook] SET  READ_WRITE 
GO
