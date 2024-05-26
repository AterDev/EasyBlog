# WinFrom中实现走马灯效果

[TOC]

在：https://www.cnblogs.com/yanjinhua/p/16611967.html 文章中作者用WPF实现了走马灯的文字效果，实现效果及相关的文字如下：

> 控件名：SpotLight
>
> 作者：WPFDevelopersOrg
>
> 原文链接： https://github.com/WPFDevelopersOrg/WPFDevelopers

实现效果

![img](https://img2022.cnblogs.com/blog/987480/202208/987480-20220822143253321-542306734.gif)

今天在WinForm中模拟实现此功能。

## 写作的目的

在WinForm中实现走马灯效果的文字。

## 实现的效果

![1](https://images.cnblogs.com/cnblogs_com/lanwah/2394700/o_240526015108_1.gif)

文字我拆分成了3行：

1、第一行，灰色文字，作为底色文字；

2、第二行，渐变色文字，作为底色之上的一层文字；

3、第三行，走马灯效果的文字。

下面进行详细的介绍。

## 实现步骤

详细介绍如何实现走马灯效果的文字。

### 第一步

绘制灰色底图文字，这一步很简单，如下图所示：
![image-20240526090923670](https://images.cnblogs.com/cnblogs_com/lanwah/2394700/o_240526015108_image-20240526090923670.png)关键代码如下：

```C#
            // 字体
            this.Font = new System.Drawing.Font("Arial Black", 50F, System.Drawing.FontStyle.Bold);
            ....
            ....
            ....
            // Step1：绘制灰色文字
            var grayBrush = new SolidBrush(ColorTranslator.FromHtml("gray"));
            g.DrawString(text, this.Font, grayBrush, x, y);
```

### 第二步

绘制渐变色顶层文字。

这步实现上来说跟WPF有比较大的区别，因为在WinForm中不支持多段的渐变画刷，在WinForm中渐变只支持起始颜色到结束色的颜色渐变，因此为了实现多段的颜色渐变就要进行多次的绘制，每次绘制特定区域的颜色渐变，用到的关键技术是：

1、`public LinearGradientBrush(RectangleF rect, Color color1, Color color2, LinearGradientMode linearGradientMode)` 创建渐变画刷；

2、Graphics 类的 `public void SetClip(RectangleF rect)` 设置剪辑区域，每次只绘制特定区域的颜色渐变，其他区域进行剪辑。

![image-20240526092259978](https://images.cnblogs.com/cnblogs_com/lanwah/2394700/o_240526015108_image-20240526092259978.png)

贴取绘制第一段渐变的代码，相关代码在代码中的【演示渐变绘制裁剪】的注释部分：

```C#
            // 第一段
            var rect = new RectangleF(x, y, width, height);
            var colorFrom = "#FF9C1031";
            var colorTo = "#FFBE0E20";
            using (var linearGradientBrush = new LinearGradientBrush(rect, ColorTranslator.FromHtml(colorFrom), ColorTranslator.FromHtml(colorTo), LinearGradientMode.Horizontal))
            {
                // 设置裁剪区域
                g.SetClip(rect);
                g.DrawString(text, this.Font, linearGradientBrush, initX, y);
                x += width;
            }
```

通过对渐变文字的拆解可知颜色渐变分成了4段，因此要进行4次绘制，绘制渐变文字的代码如下：

```C#
            // Step2：绘制渐变色文字
            x = initX;
            y = 200;

            width = (2 * step);
            x = Draw(g, text, initX, x, y, width, height, "#FF9C1031", "#FFBE0E20");

            width = (5 * step);
            x = Draw(g, text, initX, x, y, width, height, "#FFBE0E20", "#FF9C12AC");

            width = (1 * step);
            x = Draw(g, text, initX, x, y, width, height, "#FF9C12AC", "#FF0A8DC3");

            width = (2 * step);
            x = Draw(g, text, initX, x, y, width, height, "#FF0A8DC3", "#FF1AEBCC");
```

Draw函数的代码如下：

```C#
        /// <summary>
        /// 绘制渐变色文字
        /// </summary>
        /// <param name="g"></param>
        /// <param name="text"></param>
        /// <param name="initX"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="colorFrom"></param>
        /// <param name="colorTo"></param>
        /// <returns></returns>
        private float Draw(Graphics g, string text, float initX, float x, float y, float width, int height, string colorFrom, string colorTo, RectangleF moveRect = default)
        {
            var rect = new RectangleF(x, y, width, height);
            using (var linearGradientBrush = new LinearGradientBrush(rect, ColorTranslator.FromHtml(colorFrom), ColorTranslator.FromHtml(colorTo), LinearGradientMode.Horizontal))
            {
                if (moveRect == default)
                {
                    g.SetClip(rect);
                }
                else
                {
                    //绘制走马灯文字（交叉剪辑）
                    g.SetClip(moveRect);
                    g.SetClip(rect, CombineMode.Intersect);
                }
                g.DrawString(text, this.Font, linearGradientBrush, initX, y);
                x += width;
                return x;
            }
        }
```

### 第三步

实现思路：

1、把灰色文字作为底图，直接进行绘制。

2、把渐变文字作为底图的上层文字，此时如果直接绘制那么底图会被渐变色文字覆盖，因此在绘制此部分的时候需要设置个剪辑区域，为了达到动画的效果需要随着时间的变化变化剪辑区域的X坐标使其在水平方向上移动。

3、由于在绘制渐变文字的时候已经给Graphics设置了剪辑区域，再加上移动的显示部分也要设置剪辑区域因此用到了设置交叉剪辑的方法，只有两个区域交叉的部分才会显示，对其他部分进行了裁剪。

设置交叉剪辑的核心代码在Draw函数中，如下：

```C#
                    //绘制走马灯文字（交叉剪辑）
                    g.SetClip(moveRect);
                    g.SetClip(rect, CombineMode.Intersect);
```

第三步绘制的代码如下：

```C#
            // Step3：绘制走马灯文字（需要实现Step1，Step2的功能，因为文字要移动因此还采用了交叉剪辑的功能）
            x = initX;
            y = 300;
            g.ResetClip();

            // 先绘制灰色文字底图
            g.DrawString(text, this.Font, grayBrush, x, y);

            // 再绘制渐变色文字
            width = (2 * step);
            var moveRect = new RectangleF(initX + OffsetX, y, 100, height);
            x = Draw(g, text, initX, x, y, width, height, "#FF9C1031", "#FFBE0E20", moveRect);

            width = (5 * step);
            x = Draw(g, text, initX, x, y, width, height, "#FFBE0E20", "#FF9C12AC", moveRect);

            width = (1 * step);
            x = Draw(g, text, initX, x, y, width, height, "#FF9C12AC", "#FF0A8DC3", moveRect);

            width = (2 * step);
            x = Draw(g, text, initX, x, y, width, height, "#FF0A8DC3", "#FF1AEBCC", moveRect);
```

至于移动部分此中就不再进行介绍了，附上完整代码，有需要的可以参考

```C#
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpotLightDemo
{
    /// <summary>
    /// 走马灯控件
    /// </summary>
    public class SpotLightControl : Control
    {
        private Timer AnimationTimer
        {
            get; set;
        }
        public SpotLightControl()
        {
            this.Font = new System.Drawing.Font("Arial Black", 50F, System.Drawing.FontStyle.Bold);
            this.DoubleBuffered = true;
            this.Padding = new Padding(10);

            this.InitAnimationTimer();
        }

        private void InitAnimationTimer()
        {
            this.AnimationTimer = new Timer();
            this.AnimationTimer.Interval = 100;
            this.AnimationTimer.Tick += (s, e) =>
            {
                this.ChangeOffset();
            };
        }

        public int OffsetX
        {
            get; set;
        }
        private float TextWidth
        {
            get; set;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;

            float initX = this.Padding.Left;
            float initY = this.Padding.Top;
            var x = initX;
            var y = initY;
            var text = "WinFormDevelopers";

            // Step1：绘制灰色文字
            var grayBrush = new SolidBrush(ColorTranslator.FromHtml("gray"));
            g.DrawString(text, this.Font, grayBrush, x, y);

            //<GradientStop Color="#FF9C1031" Offset="0.1"/>
            //<GradientStop Color="#FFBE0E20" Offset="0.2"/>
            //<GradientStop Color="#FF9C12AC" Offset="0.7"/>
            //<GradientStop Color="#FF0A8DC3" Offset="0.8"/>
            //<GradientStop Color="#FF1AEBCC" Offset="1"/>

            var textSize = g.MeasureString(text, this.Font, 1000);
            var height = (int)textSize.Height + 1;
            var step = textSize.Width / 10F;
            this.TextWidth = textSize.Width;

            // 演示渐变绘制裁剪
            x = initX;
            y = 100;
            var width = (2 * step);

            {
                // 第一段
                var rect = new RectangleF(x, y, width, height);
                var colorFrom = "#FF9C1031";
                var colorTo = "#FFBE0E20";
                using (var linearGradientBrush = new LinearGradientBrush(rect, ColorTranslator.FromHtml(colorFrom), ColorTranslator.FromHtml(colorTo), LinearGradientMode.Horizontal))
                {
                    // 设置裁剪区域
                    g.SetClip(rect);
                    g.DrawString(text, this.Font, linearGradientBrush, initX, y);
                    x += width;
                }

                // 第二段不绘制
                width = (5 * step);
                x += width;

                // 第三段
                width = (1 * step);
                rect = new RectangleF(x, y, width, height);
                colorFrom = "#FF9C12AC";
                colorTo = "#FF0A8DC3";
                using (var linearGradientBrush = new LinearGradientBrush(rect, ColorTranslator.FromHtml(colorFrom), ColorTranslator.FromHtml(colorTo), LinearGradientMode.Horizontal))
                {
                    //// 不截取的效果
                    g.SetClip(rect);
                    g.DrawString(text, this.Font, linearGradientBrush, initX, y);
                    x += width;
                }
            }


            // Step2：绘制渐变色文字
            x = initX;
            y = 200;

            width = (2 * step);
            x = Draw(g, text, initX, x, y, width, height, "#FF9C1031", "#FFBE0E20");

            width = (5 * step);
            x = Draw(g, text, initX, x, y, width, height, "#FFBE0E20", "#FF9C12AC");

            width = (1 * step);
            x = Draw(g, text, initX, x, y, width, height, "#FF9C12AC", "#FF0A8DC3");

            width = (2 * step);
            x = Draw(g, text, initX, x, y, width, height, "#FF0A8DC3", "#FF1AEBCC");

            // Step3：绘制走马灯文字（需要实现Step1，Step2的功能，因为文字要移动因此还采用了交叉剪辑的功能）
            x = initX;
            y = 300;
            g.ResetClip();

            // 先绘制灰色文字底图
            g.DrawString(text, this.Font, grayBrush, x, y);

            // 再绘制渐变色文字
            width = (2 * step);
            var moveRect = new RectangleF(initX + OffsetX, y, 100, height);
            x = Draw(g, text, initX, x, y, width, height, "#FF9C1031", "#FFBE0E20", moveRect);

            width = (5 * step);
            x = Draw(g, text, initX, x, y, width, height, "#FFBE0E20", "#FF9C12AC", moveRect);

            width = (1 * step);
            x = Draw(g, text, initX, x, y, width, height, "#FF9C12AC", "#FF0A8DC3", moveRect);

            width = (2 * step);
            x = Draw(g, text, initX, x, y, width, height, "#FF0A8DC3", "#FF1AEBCC", moveRect);

            // 释放资源
            grayBrush.Dispose();
        }

        /// <summary>
        /// 绘制渐变色文字
        /// </summary>
        /// <param name="g"></param>
        /// <param name="text"></param>
        /// <param name="initX"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="colorFrom"></param>
        /// <param name="colorTo"></param>
        /// <returns></returns>
        private float Draw(Graphics g, string text, float initX, float x, float y, float width, int height, string colorFrom, string colorTo, RectangleF moveRect = default)
        {
            var rect = new RectangleF(x, y, width, height);
            using (var linearGradientBrush = new LinearGradientBrush(rect, ColorTranslator.FromHtml(colorFrom), ColorTranslator.FromHtml(colorTo), LinearGradientMode.Horizontal))
            {
                if (moveRect == default)
                {
                    g.SetClip(rect);
                }
                else
                {
                    //绘制走马灯文字（交叉剪辑）
                    g.SetClip(moveRect);
                    g.SetClip(rect, CombineMode.Intersect);
                }
                g.DrawString(text, this.Font, linearGradientBrush, initX, y);
                x += width;
                return x;
            }
        }

        private bool IsLeftToRight
        {
            get; set;
        } = true;

        /// <summary>
        /// 通过OffsetX，在X上加上偏移量模拟移动效果
        /// 
        /// </summary>
        private void ChangeOffset()
        {
            if (this.OffsetX >= (this.TextWidth - 100))
            {
                this.IsLeftToRight = false;
            }
            else if (this.OffsetX <= 0)
            {
                this.IsLeftToRight = true;
            }

            if (this.IsLeftToRight)
            {
                this.OffsetX += 10;
            }
            else
            {
                this.OffsetX -= 10;
            }

            this.Invalidate();
        }

        /// <summary>
        /// 开始动画
        /// </summary>
        public void StartAnimation()
        {
            this.AnimationTimer.Start();
        }

        /// <summary>
        /// 停止动画
        /// </summary>
        public void StopAnimation()
        {
            this.AnimationTimer.Stop();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.AnimationTimer?.Stop();
                this.AnimationTimer?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
```

## 结语

代码提供参考，文中只给出了实现效果和实现思路，针对特定的场景和要求可以进一步封装成需要的WinForm控件以便代码的复用。

如果大家有更好的方式和建议也欢迎给出，希望和大家一起进步。
