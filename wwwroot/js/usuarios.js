const rowsPerPage = 10;
let currentPage = 1;
const allRows = Array.from(document.querySelectorAll('#usuariosTable tbody tr'));
let rows = allRows.slice();

function renderTable() {
    rows.forEach((row, i) => {
        row.style.display = (i >= (currentPage - 1) * rowsPerPage && i < currentPage * rowsPerPage) ? '' : 'none';
    });
    renderPagination();
}

function renderPagination() {
    const totalPages = Math.ceil(rows.length / rowsPerPage);
    const pagination = document.getElementById('pagination');
    pagination.innerHTML = '';
    for (let i = 1; i <= totalPages; i++) {
        const li = document.createElement('li');
        li.className = 'page-item' + (i === currentPage ? ' active' : '');
        const a = document.createElement('a');
        a.className = 'page-link';
        a.href = '#';
        a.textContent = i;
        a.onclick = function(e) {
            e.preventDefault();
            currentPage = i;
            renderTable();
        };
        li.appendChild(a);
        pagination.appendChild(li);
    }
}

document.getElementById('searchInput').addEventListener('input', function() {
    const value = this.value.toLowerCase();
    if (value === "") {
        rows = allRows.slice();
    } else {
        rows = allRows.filter(row => {
            return row.textContent.toLowerCase().includes(value);
        });
    }
    currentPage = 1;
    renderTable();
});

renderTable();

function applyDarkTableUsuarios() {
    if (document.body.classList.contains('theme-dark')) {
        $('#usuariosTable').addClass('table-dark');
        $('#thead-usuarios').removeClass('table-light').addClass('table-dark');
        $('#usuariosTable tbody tr').css('background-color', ''); // reset
    } else {
        $('#usuariosTable').removeClass('table-dark');
        $('#thead-usuarios').removeClass('table-dark').addClass('table-light');
        $('#usuariosTable tbody tr').css('background-color', ''); // reset
    }
}
$(function(){
    applyDarkTableUsuarios();
    const observerUsuarios = new MutationObserver(applyDarkTableUsuarios);
    observerUsuarios.observe(document.body, { attributes: true, attributeFilter: ['class'] });
});
