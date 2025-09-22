// Custom validation styling for Tailwind CSS
(function () {
	try {
		if (typeof window === 'undefined') return;
		var jq = window.jQuery;
		if (!jq || !jq.validator || !jq.validator.setDefaults) return;

		jq.validator.setDefaults({
			highlight: function (element) {
				jq(element)
					.addClass('border-red-500 focus:border-red-500 focus:ring-red-500/20')
					.removeClass('border-gray-300 focus:border-wine-red focus:ring-wine-red/20');
			},
			unhighlight: function (element) {
				jq(element)
					.removeClass('border-red-500 focus:border-red-500 focus:ring-red-500/20')
					.addClass('border-gray-300 focus:border-wine-red focus:ring-wine-red/20');
			},
			errorElement: 'span',
			errorClass: 'text-red-600 text-sm mt-1',
			errorPlacement: function (error, element) {
				jq(error).insertAfter(element);
			}
		});
	} catch (e) {
		// Silently no-op: do not break page if validation libs aren't ready
	}
})();


