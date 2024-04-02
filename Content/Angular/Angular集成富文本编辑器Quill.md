# Angular集成Quill

## 需求

富文本编辑器是内容发布的常规需求，我们需要一些简单易用且具有一定自定义功能的富文本编辑器。

其中一个很重要的功能是可以上传文件到我们自己的后台服务。

## 在Angular中使用Quill

我们这里使用 [ngx-quill](https://github.com/KillerCodeMonkey/ngx-quill)来实现我们的需求。

## 安装

我们一共需要安装两个包，一个类型文件：

```json
  "dependencies": {
    // ...
    "ngx-quill": "^22.0.0",
    "quill": "^1.3.7",
  }
  "devDependencies": {
    // ...
    "@types/quill": "^1.3.10",
  }
```

## 引用

在`angular.json`中引用需要的css和js文件，如下所示

```json
// ...
"architect": {
  "build": {
      // ...
      "styles": [
          "./node_modules/@angular/material/prebuilt-themes/indigo-pink.css",
          "./node_modules/quill/dist/quill.snow.css",
          "src/styles.css"
      ],
      "scripts": [
          "./node_modules/quill/dist/quill.min.js"
      ],
      "allowedCommonJsDependencies": [
          "quill"
      ]
      },
  }

```

## 使用

在你需要使用富文本编辑器的模块中，导入模块：

```typescript
@NgModule({
  imports: [
    ...,

    QuillModule.forRoot()
  ],
  ...
})
class YourModule { ... }
```

然后使用组件 `<quill-editor></quill-editor>`。

简单的使用可直接参考官方仓库文档，我这里会着重说明一下，如何使用自定义`toolbar`，以及如何`自定义文件上传事件`。

## 自定义toolbar

自定义toolbar 可通过绑定modules去实现，直接看代码:

### 页面实现

`component.html`

```html
<quill-editor formControlName="content" [modules]="quillModules" placeholder="insert text"
  [styles]="{height:'150px'}"></quill-editor>

```

### TS

可以在任意ts文件中定义好选项内容，然后在组件中去使用它。

`cosnt.ts`

```typescript
export const QuillSimpleModules = {
  toolbar: [
    [{ 'size': ['small', false, 'large'] }],
    [{ 'color': [] }, { 'background': [] }],
    ['bold', 'italic', 'underline', 'strike', 'clean'],        // toggled buttons
    ['blockquote', 'code-block', 'link'],
    [{ 'list': 'ordered' }, { 'list': 'bullet' }],
    [{ 'direction': 'rtl' }],                         // text direction
  ]
};
```

`component.ts`

```typescript

export class DetailComponent implements OnInit {
  // ...
  quillModules = QuillSimpleModules;
  // ...
}
```

### 完整toolbar

```typescript
const modules = {
  toolbar: [
    ['bold', 'italic', 'underline', 'strike'],        // toggled buttons
    ['blockquote', 'code-block'],

    [{ 'header': 1 }, { 'header': 2 }],               // custom button values
    [{ 'list': 'ordered'}, { 'list': 'bullet' }],
    [{ 'script': 'sub'}, { 'script': 'super' }],      // superscript/subscript
    [{ 'indent': '-1'}, { 'indent': '+1' }],          // outdent/indent
    [{ 'direction': 'rtl' }],                         // text direction

    [{ 'size': ['small', false, 'large', 'huge'] }],  // custom dropdown
    [{ 'header': [1, 2, 3, 4, 5, 6, false] }],

    [{ 'color': [] }, { 'background': [] }],          // dropdown with defaults from theme
    [{ 'font': [] }],
    [{ 'align': [] }],

    ['clean'],                                         // remove formatting button

    ['link', 'image', 'video']                         // link and image, video
  ]
};
```

## 自定义文件上传

Quill通过自定义Handler去重写控件的实现。

### 添加自定义处理事件

我们将通过`onEditorCreated`函数获取Quill对象，并添加自定义事件。

`create.component.html`

```html
<quill-editor formControlName="content" [styles]="{minHeight:'500px'}"
  (onEditorCreated)="editorCreated($event)"></quill-editor>
```

使用quill的`addHandler`方法添加自定义函数。

`create.component.ts`

```typescript
export class CreateComponent implements OnInit {
  // ...
  editor!: Quill;
  // ...

  editorCreated(editor: Quill): void {
    this.editor = editor;
    this.editor.getModule('toolbar').addHandler('image', this.selectImage);
  }
}
```

实现自定义文件上传函数，我这里以上传图片为例：

`create.component.ts`

```typescript
  selectImage = () => {
    const input: any = document.createElement('input');
    input.setAttribute('type', 'file');
    input.setAttribute('accept', 'image/*')
    input.click();

    input.onchange = () => {
      const file = input.files[0];
      if (file && file.type.startsWith('image/')) {
        this.uploadImage(file);
      } else {
        console.warn('You could only upload images.');
      }
    };
  }

  uploadImage(file: File): void {
    const fd = new FormData();
    fd.append('upload', file);
    this.commonSrv.uploadImg(fd)
      .subscribe({
        next: (res) => {
          if (res) {
            const range = this.editor.getSelection(true);
            this.editor.insertEmbed(range.index, 'image', res.url);
          } else {
            this.snb.open('');
          }
        },
        error: (error) => {
          this.snb.open(error.detail);
        }
      });
  }
```

> [!TIP]
> selectImage方法使用箭头函数，这样才能有效调用当前类的方法this.uploadImage(file)。
> 上传成功后，我们使用`insertEmbed`方法向edtior中插入图片。

### 获取提交的内容

我们现在已经实现图片的上传和展示，但在我们提交表单时，会发现formControlName绑定的变量，并没有包含我们使用`insertEmbed`插入的内容。
由于安全问题，quill并不建议使用html去渲染或存储内容。如果我们确实需要最终的html，可以使用

```typescript
this.editor.root.innerHTML;
```

获取最终完整的html格式内容，然后提交到后台进行保存。
