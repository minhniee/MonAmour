// Custom validation styling for Tailwind CSS
$.validator.setDefaults({
    highlight: function (element, errorClass, validClass) {
        $(element)
            .addClass('border-red-500 focus:border-red-500 focus:ring-red-500/20')
            .removeClass('border-gray-300 focus:border-wine-red focus:ring-wine-red/20');
    },
    unhighlight: function (element, errorClass, validClass) {
        $(element)
            .removeClass('border-red-500 focus:border-red-500 focus:ring-red-500/20')
            .addClass('border-gray-300 focus:border-wine-red focus:ring-wine-red/20');
    },
    errorElement: 'span',
    errorClass: 'text-red-600 text-sm mt-1',
    errorPlacement: function (error, element) {
        error.insertAfter(element);
    }
});
