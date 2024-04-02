# VS Code 代码片段编写教程

最近要做一个vs code的代码片段插件，于是搜索和学习相关内容，整理分享给大家!

> [!TIP]
> 本篇博客50%+内容由BingChat提供，然后作者对内容进行验证和整理。

## 格式

使用json格式定义代码片段，示例:

```json
{
  "ng if": {
    "prefix": "ngif",
    "body": "",
    "description": "ng if"
  },
  "ng for": {
    "prefix": "ngfor",
    "body": "",
    "description": "ng for"
  }
}

```



## 语法：基本使用

- `$0`：表示最终光标位置。
- `${1:default}`：表示带有默认值的占位符。当代码片段展开时，光标会定位在这个占位符上，并显示默认值 "default"。
- `$CURRENT_YEAR`、`$CURRENT_MONTH`、`$CURRENT_DATE` 等：表示当前日期和时间的变量。
- `${1|one,two,three|}`：表示带有选项的占位符。当代码片段展开时，光标会定位在这个占位符上，并显示一个下拉列表，供用户选择 "one"、"two" 或 "three"。

下面是一个使用了多种占位符和变量的代码片段示例：

```json
{
    "Print date": {
        "prefix": "date",
        "body": [
            "// Current date: $CURRENT_YEAR-$CURRENT_MONTH-$CURRENT_DATE",
            "console.log('Hello, ${1:world}!');",
            "$0"
        ],
        "description": "Print current date and hello message"
    }
}
```

上面的代码定义了一个名为 "Print date" 的代码片段。它的前缀为 "date"，当你在编辑器中输入 "date" 并按下 Tab 键时，它会展开为：

```javascript
// Current date: 2022-12-01
console.log('Hello, world!');
```

其中，第一行显示了当前日期，第二行显示了一条带有默认值 "world" 的消息。光标最终会定位在 `$0` 处。

## 语法：预定义常量

- `$TM_SELECTED_TEXT`：表示当前选中的文本。
- `$TM_CURRENT_LINE`：表示当前行的内容。
- `$TM_CURRENT_WORD`：表示当前光标所在单词的内容。
- `$TM_LINE_INDEX`：表示当前行的索引（从 0 开始）。
- `$TM_LINE_NUMBER`：表示当前行号（从 1 开始）。
- `$TM_FILENAME`：表示当前文件名。
- `$TM_FILENAME_BASE`：表示当前文件名（不带扩展名）。
- `$TM_DIRECTORY`：表示当前文件所在目录。
- `$TM_FILEPATH`：表示当前文件的完整路径。

你可以在代码片段中使用这些变量来插入动态内容。例如，下面是一个使用了 `$TM_SELECTED_TEXT` 变量的代码片段：

```json
{
    "Print selected text": {
        "prefix": "printsel",
        "body": [
            "console.log('$TM_SELECTED_TEXT');",
            "$0"
        ],
        "description": "Print selected text to console"
    }
}
```

上面的代码定义了一个名为 "Print selected text" 的代码片段。它的前缀为 "printsel"，当你在编辑器中选中一些文本，然后输入 "printsel" 并按下 Tab 键时，它会展开为：

```javascript
console.log('selected text');
```

其中，"selected text" 是你之前选中的文本。

## 语法：转换

在 Visual Studio Code 的代码片段中，你可以使用转换来对变量或占位符的值进行处理。转换是一种特殊的语法，可以让你在代码片段中使用正则表达式和格式字符串来修改变量或占位符的值。

下面是一个使用转换的代码片段示例：

```json
{
    "Transform example": {
        "prefix": "transform",
        "body": [
            "Original: ${1:example}",
            "Upper case: ${1/(.*)/${1:/upcase}/}",
            "$0"
        ],
        "description": "Example of using transform in snippet"
    }
}
```

上面的代码定义了一个名为 "Transform example" 的代码片段。它的前缀为 "transform"，当你在编辑器中输入 "transform" 并按下 Tab 键时，它会展开为：

```txt
Original: example
Upper case: EXAMPLE
```

其中，第二行使用了转换语法 `${1/(.*)/${1:/upcase}/}` 来将第一个占位符的值转换为大写。转换语法中，`(.*)` 是一个正则表达式，用于匹配占位符的值；`${1:/upcase}` 是一个格式字符串，用于将匹配到的值转换为大写。

除了 `/upcase` 之外，还有其他一些预定义的格式字符串可以使用，例如 `/downcase`（转换为小写）、`/capitalize`（首字母大写）和 `/pascalcase`（帕斯卡命名法）等。

你也可以使用自定义的格式字符串来对匹配到的值进行处理。例如，下面是一个使用自定义格式字符串的代码片段示例：

```json
{
    "Custom transform example": {
        "prefix": "custom",
        "body": [
            "Original: ${1:example}",
            "Reverse: ${1/(.*)/${1[::-1]}/}",
            "$0"
        ],
        "description": "Example of using custom transform in snippet"
    }
}
```

上面的代码定义了一个名为 "Custom transform example" 的代码片段。它的前缀为 "custom"，当你在编辑器中输入 "custom" 并按下 Tab 键时，它会展开为：

```txt
Original: example
Reverse: elpmaxe
```

其中，第二行使用了转换语法 `${1/(.*)/${1[::-1]}/}` 来将第一个占位符的值反转。转换语法中，`${1[::-1]}` 是一个自定义格式字符串，用于将匹配到的值反转。

总之，在 Visual Studio Code 的代码片段中，你可以使用转换来对变量或占位符的值进行处理。你可以使用预定义或自定义的格式字符串来修改变量或占位符的值。
