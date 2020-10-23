"
" load .virmrc.local if any
"
augroup vimrc-local
  autocmd!
  autocmd BufNewFile,BufReadPost * call s:vimrc_local(expand('<afile>:p:h'))
augroup END

function! s:vimrc_local(loc)
  let files = findfile('.vimrc.local', escape(a:loc, ' ') . ';', -1)
  for i in reverse(filter(files, 'filereadable(v:val)'))
    source `=i`
  endfor
endfunction

" tab settings
set expandtab
set tabstop=2
set softtabstop=2
set shiftwidth=2

" display settings
set number
set colorcolumn=80

" file format and encoding
set encoding=utf-8
if has('unix')
  set fileformat=unix
  set fileformats=unix,dos,mac
  set fileencoding=utf-8
  set fileencodings=utf-8,iso-2022-jp,cp932,euc-jp
  set termencoding=
elseif has('win32')
  set fileformat=dos
  set fileformats=dos,unix,mac
  set fileencoding=utf-8
  set fileencodings=iso-2022-jp,utf-8,euc-jp,cp932
  set termencoding=
endif

" incremental search setting
set incsearch

" ignore case when searching
set ignorecase

" do case-sensitive search only when entering capital letters in keyword
set smartcase

" wrap-around search
set wrapscan

" high-light searched words
set hlsearch

" remove highlighting when pressing EscEsc
nmap <Esc><Esc> :nohlsearch<CR><Esc>
 
" use syntax highlighting
syntax on
