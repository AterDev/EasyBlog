#!/usr/bin/env node
import dotnet from 'node-api-dotnet';
import { Command } from "commander";
import "./bin/Share.js";
import './bin/Models.js';
import './bin/Spectre.Console.js';
import './bin/ColorCode.HTML.js';
import './bin/ColorCode.Core.js';

const Cmd = dotnet.Share.Command;

const program = new Command();

program.name('ezblog')
    .description('Generates a pure static blog website from a markdown document');

program.command('init')
    .description('初始化webinfo.json配置文件')
    .argument('[path]', '目录')
    .action((path) => {
        if (path == null) {
            path = ''
        }
        Cmd.Init(path);
    });


program.command('build')
    .description('生成静态博客站点')
    .argument('<source>', 'markdown文件目录')
    .argument('<output>', '站点输出目录')
    .action((source, output) => {
        Cmd.Build(source, output);
    });

program
    .action(() => {
        console.log('No command provided. Use --help to see available commands.');
    });

program.on('--help', () => {
    console.log('Generates a pure static blog website from a markdown document');
});

program.parse(process.argv);