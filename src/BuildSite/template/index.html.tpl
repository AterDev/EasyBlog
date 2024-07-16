<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@{Name}</title>
    <meta name="description" content="@{Description}" />
    <base href="/" />
    <link rel="stylesheet" href="@{BaseUrl}css/app.css" />
    <link rel="stylesheet" href="@{BaseUrl}css/markdown.css" />
    <link rel="icon" type="image/png" href="@{BaseUrl}favicon.ico" />
    <script>const baseUrl = '@{BaseUrl}';</script>
    <script src="@{BaseUrl}js/index.js"></script>
</head>

<body class="dark:bg-neutral-900">
    <div class="text-white py-4 bg-gray-600 dark:bg-neutral-800">
    <div class="container mx-auto flex items-center justify-between">
        <a href="@{BaseUrl}" class="text-2xl font-semibold hidden sm:block">@{Name}</a>
        <span class="hidden sm:block">@{Description}</span>
        <div class="flex items-center">
        <input id="searchText" placeholder="博客标题" class="px-4 py-2 border border-gray-600 rounded-lg dark:bg-neutral-800 text-black dark:text-white focus:outline-none" />
        <button id="searchBtn" class="ml-2 bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 rounded-lg">
            搜索
        </button>
        </div>
    </div>
    </div>

    <div class="container mx-auto mt-2">
    <div class="flex">
        <div class="sm:w-3/4 sm:pr-4 w-full">
        <!-- 博客卡片列表 -->
        <div class="px-3 bg-white dark:bg-neutral-900" id="blogList">
            <!-- 博客卡片内容 -->
            @{blogList}
        </div>
    </div>

    <div class="w-1/4 mt-1 hidden sm:block">
        <!-- 分类 -->
        @{siderbar}
    </div>
    </div>
    </div>

    <div class="dark:bg-neutral-800 py-4 fixed bottom-0 w-full bg-gray-200">
    <div class="container mx-auto text-center">
        <p class="text-neutral-600 dark:text-neutral-300">
        @{Name}
        <a class="text-blue-600" target="_blank" href="https://github.com/AterDev/EasyBlog">Powered by Ater Blog</a>
        </p>
    </div>
    </div>
</body>
</html>