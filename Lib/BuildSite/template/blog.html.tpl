<!DOCTYPE html>
<html>
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <link rel="stylesheet" href="@{BaseUrl}css/app.css">
    <link rel="stylesheet" href="@{BaseUrl}css/markdown.css">
    <link rel="icon" type="image/png" href="@{BaseUrl}favicon.ico" />
    <script src="@{BaseUrl}js/markdown.js"></script>
    <title>@{Title}</title>
    @{ExtensionHead}
</head>
<body class="container mx-auto px-4 sm:px-6 lg:px-8 dark:bg-neutral-900 pb-4">
    <div class="flex mt-2">
        <div class="sm:w-3/4 sm:pr-4 w-full">
        @{content}
        </div>
        <div class="w-1/4 mt-1 hidden sm:flex">
            <div class="toc-block sticky top-2">
                @{toc}
            </div>
        </div>
    </div>
</body>
</html>