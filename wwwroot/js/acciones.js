// Soporte de modo oscuro para tabla de acciones
function applyDarkTableAcciones() {
    if (document.body.classList.contains('theme-dark')) {
        $('#tabla-acciones').addClass('table-dark');
        $('#thead-acciones').removeClass('table-light').addClass('table-dark');
        $('#tabla-acciones tbody tr').css('background-color', ''); // reset
    } else {
        $('#tabla-acciones').removeClass('table-dark');
        $('#thead-acciones').removeClass('table-dark').addClass('table-light');
        $('#tabla-acciones tbody tr').css('background-color', ''); // reset
    }
}
$(function(){
    applyDarkTableAcciones();
    const observerAcciones = new MutationObserver(applyDarkTableAcciones);
    observerAcciones.observe(document.body, { attributes: true, attributeFilter: ['class'] });
});
