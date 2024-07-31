#!/usr/bin/env node
import dotnet from 'node-api-dotnet';
import { Command } from "commander";


const program = new Command();

program.name('ezblog')
    .description('Generates a pure static blog website from a markdown document');

program.command('init')
    .description('')
    .action(() => {
        console.log('init');
    });


program.command('build <source> <output>')
    .description('生成静态博客站点')
    .argument('<source>', 'markdown文件目录')
    .argument('<output>', '站点输出目录')
    .action((source, output) => {

    });

program
    .action(() => {
        console.log('No command provided. Use --help to see available commands.');
    });

program.on('--help', () => {
    console.log('Generates a pure static blog website from a markdown document');
});

program.parse(process.argv);